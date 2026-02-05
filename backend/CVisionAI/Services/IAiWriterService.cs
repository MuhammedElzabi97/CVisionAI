using System;
using System.Threading.Tasks;
using CVisionAI.Domain.Entities;
using CVisionAI.Models.Dtos;

namespace CVisionAI.Services
{
    public interface IAiWriterService
    {
        Task<AiOptimizeExperienceResponseDto> OptimizeExperienceAsync(Profile profile, Experience experience, AiOptimizeExperienceRequestDto request);
    }
}

