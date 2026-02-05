using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CVisionAI.Data;
using CVisionAI.Domain.Entities;
using CVisionAI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CVisionAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProfilesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProfileResponseDto>> GetById(Guid id)
        {
            var profile = await _db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null)
            {
                return NotFound();
            }

            var dto = MapToResponse(profile);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ProfileResponseDto>> Create([FromBody] ProfileCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var profile = new Profile
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Location = dto.Location,
                LinksJson = dto.Links is null ? null : JsonSerializer.Serialize(dto.Links),
                Summary = dto.Summary,
                SkillsJson = dto.Skills is null ? null : JsonSerializer.Serialize(dto.Skills),
                TargetRole = dto.TargetRole,
                Language = dto.Language,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Profiles.Add(profile);
            await _db.SaveChangesAsync();

            var response = MapToResponse(profile);
            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProfileResponseDto>> Update(Guid id, [FromBody] ProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null)
            {
                return NotFound();
            }

            profile.FullName = dto.FullName;
            profile.Email = dto.Email;
            profile.Phone = dto.Phone;
            profile.Location = dto.Location;
            profile.LinksJson = dto.Links is null ? null : JsonSerializer.Serialize(dto.Links);
            profile.Summary = dto.Summary;
            profile.SkillsJson = dto.Skills is null ? null : JsonSerializer.Serialize(dto.Skills);
            profile.TargetRole = dto.TargetRole;
            profile.Language = dto.Language;
            profile.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var response = MapToResponse(profile);
            return Ok(response);
        }

        private static ProfileResponseDto MapToResponse(Profile profile)
        {
            var links = string.IsNullOrWhiteSpace(profile.LinksJson)
                ? null
                : JsonSerializer.Deserialize<System.Collections.Generic.List<LinkDto>>(profile.LinksJson);

            var skills = string.IsNullOrWhiteSpace(profile.SkillsJson)
                ? null
                : JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(profile.SkillsJson);

            return new ProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.FullName,
                Email = profile.Email,
                Phone = profile.Phone,
                Location = profile.Location,
                Links = links,
                Summary = profile.Summary,
                Skills = skills,
                TargetRole = profile.TargetRole,
                Language = profile.Language,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
    }
}

