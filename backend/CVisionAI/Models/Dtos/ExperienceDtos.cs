using System;
using System.ComponentModel.DataAnnotations;

namespace CVisionAI.Models.Dtos
{
    public class ExperienceCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Company { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }
    }

    public class ExperienceUpdateDto : ExperienceCreateDto
    {
    }

    public class ExperienceResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

