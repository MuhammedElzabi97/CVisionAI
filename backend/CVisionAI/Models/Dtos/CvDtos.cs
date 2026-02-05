using System;
using System.Collections.Generic;

namespace CVisionAI.Models.Dtos
{
    public class AtsMissingKeywordDto
    {
        public string Keyword { get; set; } = string.Empty;
        public string Impact { get; set; } = "MEDIUM";
    }

    public class AtsReportDto
    {
        public int OverallScore { get; set; }
        public int KeywordMatch { get; set; }
        public int Formatting { get; set; }
        public int Readability { get; set; }
        public List<AtsMissingKeywordDto> MissingKeywords { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }

    public class GenerateCvRequestDto
    {
        public Guid ProfileId { get; set; }
        public Guid TemplateId { get; set; }
        public string TargetRole { get; set; } = string.Empty;
        public string Language { get; set; } = "EN";
        public string? JobDescriptionText { get; set; }
    }

    public class GeneratedFilesDto
    {
        public string? PdfUrl { get; set; }
        public string? DocxUrl { get; set; }
    }

    public class GeneratedCvResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public Guid TemplateId { get; set; }
        public string HtmlPreview { get; set; } = string.Empty;
        public AtsReportDto AtsReport { get; set; } = new();
        public GeneratedFilesDto Files { get; set; } = new();
    }
}

