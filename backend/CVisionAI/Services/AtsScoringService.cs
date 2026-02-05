using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CVisionAI.Domain.Entities;
using CVisionAI.Models.Dtos;

namespace CVisionAI.Services
{
    /// <summary>
    /// Simple rule-based ATS scoring for MVP.
    /// </summary>
    public class AtsScoringService : IAtsScoringService
    {
        public Task<AtsReportDto> ScoreAsync(GeneratedCv generatedCv, string jobDescriptionText)
        {
            var html = generatedCv.HtmlPreview ?? string.Empty;
            var plain = StripHtml(html).ToLowerInvariant();

            var jd = jobDescriptionText ?? string.Empty;
            var jdPlain = jd.ToLowerInvariant();

            // Extract keywords from job description (very naive split on non-letters)
            var jdTokens = Regex.Split(jdPlain, @"[^a-zA-Z0-9\+]+")
                .Where(t => t.Length > 2)
                .Distinct()
                .ToList();

            var present = new HashSet<string>();
            var missing = new List<AtsMissingKeywordDto>();

            foreach (var token in jdTokens)
            {
                if (plain.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    present.Add(token);
                }
                else
                {
                    missing.Add(new AtsMissingKeywordDto
                    {
                        Keyword = token,
                        Impact = "MEDIUM"
                    });
                }
            }

            int keywordMatch = jdTokens.Count == 0
                ? 100
                : (int)Math.Round(100.0 * present.Count / jdTokens.Count);

            // Formatting: penalize tables or multi-column layout hints
            int formattingScore = 100;
            if (html.Contains("<table", StringComparison.OrdinalIgnoreCase))
            {
                formattingScore -= 20;
            }

            // Readability: simple heuristic based on sentence length
            var sentences = plain.Split('.', '!', '?')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();

            int readability = 100;
            if (sentences.Count > 0)
            {
                var avgLen = sentences.Average(s => s.Length);
                if (avgLen > 200) readability = 60;
                else if (avgLen > 120) readability = 75;
                else if (avgLen > 80) readability = 85;
            }

            var overall = (int)Math.Round(0.5 * keywordMatch + 0.3 * formattingScore + 0.2 * readability);

            var report = new AtsReportDto
            {
                OverallScore = overall,
                KeywordMatch = keywordMatch,
                Formatting = formattingScore,
                Readability = readability,
                MissingKeywords = missing,
                Notes = new List<string>
                {
                    "This is a simple heuristic ATS score for MVP."
                }
            };

            return Task.FromResult(report);
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}

