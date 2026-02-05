using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CVisionAI.Models.Dtos
{
    public class LinkDto
    {
        [Required]
        public string Label { get; set; } = string.Empty;

        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;
    }

    public class ProfileCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public List<LinkDto>? Links { get; set; }

        public string? Summary { get; set; }

        public List<string>? Skills { get; set; }

        [MaxLength(100)]
        public string? TargetRole { get; set; }

        [MaxLength(10)]
        public string Language { get; set; } = "EN"; // "TR" | "EN"
    }

    public class ProfileUpdateDto : ProfileCreateDto
    {
    }

    public class ProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public List<LinkDto>? Links { get; set; }
        public string? Summary { get; set; }
        public List<string>? Skills { get; set; }
        public string? TargetRole { get; set; }
        public string Language { get; set; } = "EN";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

