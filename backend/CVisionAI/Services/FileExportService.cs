using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CVisionAI.Domain.Entities;

namespace CVisionAI.Services
{
    /// <summary>
    /// Very simple file export service.
    /// For MVP we just persist the HTML into .pdf/.docx-named files without real conversion.
    /// This is enough to test the end-to-end flow and URLs.
    /// </summary>
    public class FileExportService : IExportService
    {
        public Task<GeneratedFile> GeneratePdfAsync(GeneratedCv generatedCv, string htmlContent, string storageRoot)
        {
            return Task.FromResult(WriteFile(generatedCv, htmlContent, storageRoot, "pdf"));
        }

        public Task<GeneratedFile> GenerateDocxAsync(GeneratedCv generatedCv, string htmlContent, string storageRoot)
        {
            return Task.FromResult(WriteFile(generatedCv, htmlContent, storageRoot, "docx"));
        }

        private GeneratedFile WriteFile(GeneratedCv generatedCv, string htmlContent, string storageRoot, string extension)
        {
            var cvFolder = Path.Combine(storageRoot, generatedCv.Id.ToString("N"));
            if (!Directory.Exists(cvFolder))
            {
                Directory.CreateDirectory(cvFolder);
            }

            var fileName = $"cv_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
            var filePath = Path.Combine(cvFolder, fileName);

            File.WriteAllText(filePath, htmlContent ?? string.Empty, Encoding.UTF8);

            var relativeUrl = $"/storage/{generatedCv.Id:N}/{fileName}";

            return new GeneratedFile
            {
                GeneratedCvId = generatedCv.Id,
                FileType = extension.ToUpperInvariant(),
                FilePath = filePath,
                PublicUrl = relativeUrl,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}

