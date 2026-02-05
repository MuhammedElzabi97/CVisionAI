using System;
using System.Linq;
using System.Text.Json;
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
    [Route("api/[controller]")]
    public class CvController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ICvRenderer _renderer;
        private readonly IAtsScoringService _ats;
        private readonly IExportService _exportService;
        private readonly IWebHostEnvironment _env;

        public CvController(
            ApplicationDbContext db,
            ICvRenderer renderer,
            IAtsScoringService ats,
            IExportService exportService,
            IWebHostEnvironment env)
        {
            _db = db;
            _renderer = renderer;
            _ats = ats;
            _exportService = exportService;
            _env = env;
        }

        // POST /api/cv/generate
        [HttpPost("generate")]
        public async Task<ActionResult<GeneratedCvResponseDto>> Generate([FromBody] GenerateCvRequestDto request)
        {
            var profile = await _db.Profiles.Include(p => p.Experiences).FirstOrDefaultAsync(p => p.Id == request.ProfileId);
            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            var template = await _db.Templates.FirstOrDefaultAsync(t => t.Id == request.TemplateId);
            if (template == null)
            {
                return NotFound("Template not found.");
            }

            var html = await _renderer.RenderHtmlAsync(
                profile,
                profile.Experiences,
                template,
                request.TargetRole,
                request.Language);

            var generated = new GeneratedCv
            {
                ProfileId = profile.Id,
                TemplateId = template.Id,
                Title = $"{request.TargetRole} CV",
                TargetRole = request.TargetRole,
                Language = request.Language,
                HtmlPreview = html,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.GeneratedCvs.Add(generated);
            await _db.SaveChangesAsync();

            var atsReport = await _ats.ScoreAsync(generated, request.JobDescriptionText ?? string.Empty);
            generated.AtsReportJson = JsonSerializer.Serialize(atsReport);
            generated.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var response = MapToResponseDto(generated, atsReport, null, null);
            return Ok(response);
        }

        // GET /api/cv/{cvId}
        [HttpGet("{cvId:guid}")]
        public async Task<ActionResult<GeneratedCvResponseDto>> Get(Guid cvId)
        {
            var generated = await _db.GeneratedCvs.FirstOrDefaultAsync(g => g.Id == cvId);
            if (generated == null)
            {
                return NotFound();
            }

            AtsReportDto ats;
            if (!string.IsNullOrWhiteSpace(generated.AtsReportJson))
            {
                ats = JsonSerializer.Deserialize<AtsReportDto>(generated.AtsReportJson) ?? new AtsReportDto();
            }
            else
            {
                ats = new AtsReportDto();
            }

            var files = await _db.GeneratedFiles.Where(f => f.GeneratedCvId == cvId).ToListAsync();
            var pdfUrl = files.FirstOrDefault(f => f.FileType == "PDF")?.PublicUrl;
            var docxUrl = files.FirstOrDefault(f => f.FileType == "DOCX")?.PublicUrl;

            var response = MapToResponseDto(generated, ats, pdfUrl, docxUrl);
            return Ok(response);
        }

        // POST /api/cv/{cvId}/export/pdf
        [HttpPost("{cvId:guid}/export/pdf")]
        public async Task<ActionResult<object>> ExportPdf(Guid cvId)
        {
            var generated = await _db.GeneratedCvs.FirstOrDefaultAsync(g => g.Id == cvId);
            if (generated == null)
            {
                return NotFound();
            }

            var storageRoot = Path.Combine(_env.ContentRootPath, "storage");
            var file = await _exportService.GeneratePdfAsync(generated, generated.HtmlPreview ?? string.Empty, storageRoot);
            _db.GeneratedFiles.Add(file);
            await _db.SaveChangesAsync();

            return Ok(new { url = file.PublicUrl });
        }

        // POST /api/cv/{cvId}/export/docx
        [HttpPost("{cvId:guid}/export/docx")]
        public async Task<ActionResult<object>> ExportDocx(Guid cvId)
        {
            var generated = await _db.GeneratedCvs.FirstOrDefaultAsync(g => g.Id == cvId);
            if (generated == null)
            {
                return NotFound();
            }

            var storageRoot = Path.Combine(_env.ContentRootPath, "storage");
            var file = await _exportService.GenerateDocxAsync(generated, generated.HtmlPreview ?? string.Empty, storageRoot);
            _db.GeneratedFiles.Add(file);
            await _db.SaveChangesAsync();

            return Ok(new { url = file.PublicUrl });
        }

        private static GeneratedCvResponseDto MapToResponseDto(
            GeneratedCv g,
            AtsReportDto ats,
            string? pdfUrl,
            string? docxUrl)
        {
            return new GeneratedCvResponseDto
            {
                Id = g.Id,
                Title = g.Title,
                UpdatedAt = g.UpdatedAt,
                TemplateId = g.TemplateId,
                HtmlPreview = g.HtmlPreview ?? string.Empty,
                AtsReport = ats,
                Files = new GeneratedFilesDto
                {
                    PdfUrl = pdfUrl,
                    DocxUrl = docxUrl
                }
            };
        }
    }
}

