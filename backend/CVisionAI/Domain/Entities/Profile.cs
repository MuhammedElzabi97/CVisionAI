using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CVisionAI.Domain.Entities
{
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Optional: Link to Identity User if we add auth later
        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        // Storing JSON string for links (e.g. [{"label":"GitHub","url":"..."}])
        public string? LinksJson { get; set; }

        public string? Summary { get; set; }

        // Storing JSON array string for skills
        public string? SkillsJson { get; set; }

        [MaxLength(100)]
        public string? TargetRole { get; set; }

        [MaxLength(10)]
        public string Language { get; set; } = "EN"; // "TR" or "EN"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    }
}
