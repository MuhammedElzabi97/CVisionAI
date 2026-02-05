using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CVisionAI.Models.Dtos
{
    public class AiOptimizeExperienceRequestDto
    {
        public Guid? ProfileId { get; set; }

        public Guid? ExperienceId { get; set; }

        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Company { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string TargetRole { get; set; } = string.Empty;

        public string? JobDescriptionText { get; set; }

        [MaxLength(10)]
        public string Language { get; set; } = "EN";
    }

    public class AiOptimizeExperienceResponseDto
    {
        public string OptimizedDescription { get; set; } = string.Empty;
        public List<string> SuggestedBullets { get; set; } = new();
    }
}

