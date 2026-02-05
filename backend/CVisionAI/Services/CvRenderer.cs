using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CVisionAI.Domain.Entities;

namespace CVisionAI.Services
{
    /// <summary>
    /// Minimal HTML renderer with inline CSS per spec.
    /// Currently implements a single-column ATS-friendly layout.
    /// </summary>
    public class CvRenderer : ICvRenderer
    {
        public Task<string> RenderHtmlAsync(
            Profile profile,
            IEnumerable<Experience> experiences,
            Template template,
            string targetRole,
            string language)
        {
            var expList = experiences
                .OrderByDescending(e => e.StartDate)
                .ToList();

            var sb = new StringBuilder();

            sb.Append("""
<html>
  <head>
    <meta charset="utf-8"/>
    <style>
      body { font-family: Arial, sans-serif; margin: 32px; }
      h1 { margin-bottom: 4px; }
      h2 { margin-top: 16px; font-size: 14px; text-transform: uppercase; }
      .muted { color: #666; font-size: 12px; }
      ul { padding-left: 20px; }
      li { margin-bottom: 4px; }
    </style>
  </head>
  <body>
""");

            sb.AppendLine($"    <h1>{System.Net.WebUtility.HtmlEncode(profile.FullName)}</h1>");

            var contactParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(profile.Location)) contactParts.Add(profile.Location);
            if (!string.IsNullOrWhiteSpace(profile.Email)) contactParts.Add(profile.Email);
            if (!string.IsNullOrWhiteSpace(profile.Phone)) contactParts.Add(profile.Phone);

            sb.AppendLine($"    <div class=\"muted\">{System.Net.WebUtility.HtmlEncode(string.Join(" • ", contactParts))}</div>");

            if (!string.IsNullOrWhiteSpace(profile.Summary))
            {
                sb.AppendLine("    <h2>Summary</h2>");
                sb.AppendLine($"    <p>{System.Net.WebUtility.HtmlEncode(profile.Summary)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(profile.SkillsJson))
            {
                try
                {
                    var skills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(profile.SkillsJson);
                    if (skills != null && skills.Count > 0)
                    {
                        sb.AppendLine("    <h2>Skills</h2>");
                        sb.AppendLine($"    <p>{System.Net.WebUtility.HtmlEncode(string.Join(", ", skills))}</p>");
                    }
                }
                catch
                {
                    // ignore malformed JSON in MVP
                }
            }

            if (expList.Count > 0)
            {
                sb.AppendLine("    <h2>Experience</h2>");
                sb.AppendLine("    <ul>");
                foreach (var e in expList)
                {
                    var dateRange = $"{FormatDate(e.StartDate)} - {(e.EndDate.HasValue ? FormatDate(e.EndDate.Value) : "Present")}";
                    sb.AppendLine("      <li>");
                    sb.AppendLine($"        <strong>{System.Net.WebUtility.HtmlEncode(e.JobTitle)}</strong> at {System.Net.WebUtility.HtmlEncode(e.Company)} ({dateRange})");
                    if (!string.IsNullOrWhiteSpace(e.Description))
                    {
                        sb.AppendLine("<br/>");
                        sb.AppendLine(System.Net.WebUtility.HtmlEncode(e.Description));
                    }
                    sb.AppendLine("      </li>");
                }
                sb.AppendLine("    </ul>");
            }

            sb.AppendLine("  </body>");
            sb.AppendLine("</html>");

            return Task.FromResult(sb.ToString());
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("MMM yyyy", CultureInfo.InvariantCulture);
        }
    }
}

