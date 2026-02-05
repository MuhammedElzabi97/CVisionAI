using System;
using System.Threading.Tasks;
using CVisionAI.Domain.Entities;

namespace CVisionAI.Services
{
    public interface IExportService
    {
        Task<GeneratedFile> GeneratePdfAsync(GeneratedCv generatedCv, string htmlContent, string storageRoot);
        Task<GeneratedFile> GenerateDocxAsync(GeneratedCv generatedCv, string htmlContent, string storageRoot);
    }
}

