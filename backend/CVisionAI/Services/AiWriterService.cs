using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CVisionAI.Domain.Entities;
using CVisionAI.Models.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CVisionAI.Services
{
    /// <summary>
    /// AI writer service that calls OpenAI (or compatible) chat completions API
    /// to optimize an experience description for ATS-friendly CVs.
    ///
    /// If no API key is configured, it falls back to a simple local transformation
    /// so that the endpoint still works in development without secrets.
    /// </summary>
    public class AiWriterService : IAiWriterService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiWriterService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public AiWriterService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiWriterService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AiOptimizeExperienceResponseDto> OptimizeExperienceAsync(
            Profile profile,
            Experience experience,
            AiOptimizeExperienceRequestDto request)
        {
            var apiKey = _configuration["OpenAI:ApiKey"] ??
                         Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            var model = _configuration["OpenAI:Model"] ?? "gpt-4.1-mini";

            // If there is no API key configured, fall back to simple local behavior.
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("OpenAI API key is not configured. Falling back to local stub behavior.");
                return BuildLocalFallback(profile, experience, request);
            }

            try
            {
                using var requestMessage = new HttpRequestMessage(
                    HttpMethod.Post,
                    "v1/chat/completions");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var systemPrompt =
                    "You are an expert CV writer and ATS (Applicant Tracking System) specialist. " +
                    "You rewrite one work experience entry to be concise, action-verb driven, and ATS-friendly. " +
                    "Use standard CV language, avoid emojis, avoid tables or multi-column layouts. " +
                    "Focus on quantified impact (%, time saved, revenue, users, etc.). " +
                    "Return a pure JSON object with fields: optimizedDescription (string) and suggestedBullets (string[]).";

                var userPrompt = BuildUserPrompt(profile, experience, request);

                var payload = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.4,
                    max_tokens = 400
                };

                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                var chatResponse = await JsonSerializer.DeserializeAsync<OpenAiChatResponse>(stream, _jsonOptions);

                var content = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("OpenAI response had empty content. Falling back to local stub.");
                    return BuildLocalFallback(profile, experience, request);
                }

                // The model is instructed to return pure JSON.
                AiOptimizeExperienceResponseDto? aiResult = null;
                try
                {
                    aiResult = JsonSerializer.Deserialize<AiOptimizeExperienceResponseDto>(content, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse OpenAI JSON content. Content: {Content}", content);
                }

                aiResult ??= BuildLocalFallback(profile, experience, request);

                // Ensure bullets list is not null
                aiResult.SuggestedBullets ??= new List<string>();

                return aiResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling OpenAI API. Falling back to local stub behavior.");
                return BuildLocalFallback(profile, experience, request);
            }
        }

        private static string BuildUserPrompt(Profile profile, Experience experience, AiOptimizeExperienceRequestDto request)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Language: {request.Language}");
            sb.AppendLine($"Target role: {request.TargetRole}");
            sb.AppendLine();
            sb.AppendLine("Candidate profile (high-level):");
            sb.AppendLine($"Full name: {profile.FullName}");
            if (!string.IsNullOrWhiteSpace(profile.Summary))
            {
                sb.AppendLine($"Summary: {profile.Summary}");
            }

            sb.AppendLine();
            sb.AppendLine("Existing experience entry to optimize:");
            sb.AppendLine($"Job title: {experience.JobTitle}");
            sb.AppendLine($"Company: {experience.Company}");
            var periodEnd = experience.EndDate.HasValue
                ? experience.EndDate.Value.ToString("yyyy-MM-dd")
                : "Present";
            sb.AppendLine($"Period: {experience.StartDate:yyyy-MM-dd} - {periodEnd}");
            sb.AppendLine("Description:");
            sb.AppendLine(experience.Description ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(request.JobDescriptionText))
            {
                sb.AppendLine();
                sb.AppendLine("Job description / posting text (for keyword matching):");
                sb.AppendLine(request.JobDescriptionText);
            }

            sb.AppendLine();
            sb.AppendLine("Instructions:");
            sb.AppendLine("- Rewrite the description for ATS-friendly CVs.");
            sb.AppendLine("- Use strong action verbs and quantify impact where possible.");
            sb.AppendLine("- Do NOT add tables, emojis, or fancy formatting.");
            sb.AppendLine("- Keep it suitable for a single-column CV layout.");
            sb.AppendLine("- Return only JSON: { \"optimizedDescription\": string, \"suggestedBullets\": string[] }.");

            return sb.ToString();
        }

        private static AiOptimizeExperienceResponseDto BuildLocalFallback(
            Profile profile,
            Experience experience,
            AiOptimizeExperienceRequestDto request)
        {
            var original = experience.Description ?? string.Empty;

            var optimized = $"[{request.TargetRole} | {request.Language}] {original}";

            var bullets = original
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (bullets.Count == 0 && !string.IsNullOrWhiteSpace(original))
            {
                bullets.Add(original);
            }

            return new AiOptimizeExperienceResponseDto
            {
                OptimizedDescription = optimized,
                SuggestedBullets = bullets
            };
        }

        // Minimal models for OpenAI chat completion response
        private sealed class OpenAiChatResponse
        {
            [JsonPropertyName("choices")]
            public List<OpenAiChoice> Choices { get; set; } = new();
        }

        private sealed class OpenAiChoice
        {
            [JsonPropertyName("message")]
            public OpenAiMessage? Message { get; set; }
        }

        private sealed class OpenAiMessage
        {
            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }
    }
}

