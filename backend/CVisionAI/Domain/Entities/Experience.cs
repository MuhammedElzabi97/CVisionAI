using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CVisionAI.Domain.Entities
{
    public class Experience
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProfileId { get; set; }
        [JsonIgnore] // Prevent cycles if serializing directly
        public Profile? Profile { get; set; }

        [Required]
        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Company { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        
        // Null means "Present"
        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
