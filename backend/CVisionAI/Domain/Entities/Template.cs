using System;
using System.ComponentModel.DataAnnotations;

namespace CVisionAI.Domain.Entities
{
    public class Template
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "ATS_MINIMAL"; // "ATS_MINIMAL" | "CREATIVE" | "ACADEMIC"

        public int AtsScoreHint { get; set; }

        [MaxLength(200)]
        public string? Subtitle { get; set; }

        [Required]
        [MaxLength(50)]
        // Key to choose the HTML layout in the renderer
        public string HtmlLayoutKey { get; set; } = "ats_minimal"; 
    }
}
