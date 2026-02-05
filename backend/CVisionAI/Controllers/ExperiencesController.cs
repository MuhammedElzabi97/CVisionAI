using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVisionAI.Data;
using CVisionAI.Domain.Entities;
using CVisionAI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CVisionAI.Controllers
{
    [ApiController]
    public class ExperiencesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ExperiencesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/profiles/{profileId}/experiences
        [HttpGet("api/profiles/{profileId:guid}/experiences")]
        public async Task<ActionResult<IEnumerable<ExperienceResponseDto>>> GetForProfile(Guid profileId)
        {
            var exists = await _db.Profiles.AnyAsync(p => p.Id == profileId);
            if (!exists)
            {
                return NotFound();
            }

            var experiences = await _db.Experiences
                .Where(e => e.ProfileId == profileId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            var result = experiences.Select(MapToResponse).ToList();
            return Ok(result);
        }

        // POST /api/profiles/{profileId}/experiences
        [HttpPost("api/profiles/{profileId:guid}/experiences")]
        public async Task<ActionResult<ExperienceResponseDto>> Create(Guid profileId, [FromBody] ExperienceCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var exists = await _db.Profiles.AnyAsync(p => p.Id == profileId);
            if (!exists)
            {
                return NotFound();
            }

            var experience = new Experience
            {
                ProfileId = profileId,
                JobTitle = dto.JobTitle,
                Company = dto.Company,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Experiences.Add(experience);
            await _db.SaveChangesAsync();

            var response = MapToResponse(experience);
            return CreatedAtAction(nameof(GetForProfile), new { profileId }, response);
        }

        // PUT /api/experiences/{experienceId}
        [HttpPut("api/experiences/{experienceId:guid}")]
        public async Task<ActionResult<ExperienceResponseDto>> Update(Guid experienceId, [FromBody] ExperienceUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var experience = await _db.Experiences.FirstOrDefaultAsync(e => e.Id == experienceId);
            if (experience == null)
            {
                return NotFound();
            }

            experience.JobTitle = dto.JobTitle;
            experience.Company = dto.Company;
            experience.StartDate = dto.StartDate;
            experience.EndDate = dto.EndDate;
            experience.Description = dto.Description;
            experience.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var response = MapToResponse(experience);
            return Ok(response);
        }

        // DELETE /api/experiences/{experienceId}
        [HttpDelete("api/experiences/{experienceId:guid}")]
        public async Task<IActionResult> Delete(Guid experienceId)
        {
            var experience = await _db.Experiences.FirstOrDefaultAsync(e => e.Id == experienceId);
            if (experience == null)
            {
                return NotFound();
            }

            _db.Experiences.Remove(experience);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static ExperienceResponseDto MapToResponse(Experience e)
        {
            return new ExperienceResponseDto
            {
                Id = e.Id,
                ProfileId = e.ProfileId,
                JobTitle = e.JobTitle,
                Company = e.Company,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };
        }
    }
}

