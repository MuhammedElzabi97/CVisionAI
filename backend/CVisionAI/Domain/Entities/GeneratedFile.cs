using System;
using System.ComponentModel.DataAnnotations;

namespace CVisionAI.Domain.Entities
{
    public class GeneratedFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GeneratedCvId { get; set; }
        // Navigation not strictly necessary for simple retrieval, but good practice
        // public GeneratedCv? GeneratedCv { get; set; }

        [MaxLength(10)]
        public string FileType { get; set; } = "PDF"; // "PDF" | "DOCX"

        public string? FilePath { get; set; }

        public string? PublicUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
