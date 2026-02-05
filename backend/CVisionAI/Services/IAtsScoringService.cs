using System.Threading.Tasks;
using CVisionAI.Domain.Entities;
using CVisionAI.Models.Dtos;

namespace CVisionAI.Services
{
    public interface IAtsScoringService
    {
        Task<AtsReportDto> ScoreAsync(GeneratedCv generatedCv, string jobDescriptionText);
    }
}

