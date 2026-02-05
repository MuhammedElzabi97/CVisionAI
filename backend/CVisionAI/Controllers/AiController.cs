using System;
using System.Threading.Tasks;
using CVisionAI.Data;
using CVisionAI.Domain.Entities;
using CVisionAI.Models.Dtos;
using CVisionAI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CVisionAI.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IAiWriterService _aiWriter;

        public AiController(ApplicationDbContext db, IAiWriterService aiWriter)
        {
            _db = db;
            _aiWriter = aiWriter;
        }

        [HttpPost("optimize/experience")]
        public async Task<ActionResult<AiOptimizeExperienceResponseDto>> OptimizeExperience([FromBody] AiOptimizeExperienceRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            Profile profile = null;
            Experience experience = null;

            // 1. Try to fetch by ID if provided
            if (request.ProfileId.HasValue)
            {
                profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == request.ProfileId.Value);
            }

            // 2. If no profile found (or not provided), create a transient one
            if (profile == null)
            {
                profile = new Profile
                {
                    FullName = "Draft Candidate",
                    TargetRole = request.TargetRole,
                    Language = request.Language
                };
            }

            // 3. Try to fetch experience by ID if provided
            if (request.ExperienceId.HasValue && request.ProfileId.HasValue)
            {
                experience = await _db.Experiences.FirstOrDefaultAsync(e => e.Id == request.ExperienceId.Value && e.ProfileId == request.ProfileId.Value);
            }

            // 4. If no experience found (or not provided), create a transient one from request DTO
            if (experience == null)
            {
                experience = new Experience
                {
                    JobTitle = request.JobTitle,
                    Company = request.Company,
                    Description = request.Description,
                    StartDate = DateTime.UtcNow, // Dummy date
                    ProfileId = profile.Id
                };
            }

            // 5. Override/Merge prompt data if needed (e.g. if user is editing a drafted description)
            // If the user sent a specific description in the request (even if using an existing ID), 
            // we might want to prioritize the request's current text over the DB text.
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                experience.Description = request.Description;
            }

            var result = await _aiWriter.OptimizeExperienceAsync(profile, experience, request);
            return Ok(result);
        }
    }
}

