using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CVisionAI.Domain.Entities
{
    public class GeneratedCv
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProfileId { get; set; }
        public Profile? Profile { get; set; }

        public Guid TemplateId { get; set; }
        public Template? Template { get; set; }

        [MaxLength(100)]
        public string Title { get; set; } = "My CV";

        [MaxLength(100)]
        public string TargetRole { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Language { get; set; } = "EN";

        // Storing the HTML content or path to it
        public string? HtmlPreview { get; set; }

        // Storing the ATS analysis report as JSON
        public string? AtsReportJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
