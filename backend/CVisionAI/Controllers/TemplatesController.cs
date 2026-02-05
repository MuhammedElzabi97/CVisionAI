using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVisionAI.Data;
using CVisionAI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CVisionAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TemplatesController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns available CV templates.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            var templates = await _db.Templates.AsNoTracking().ToListAsync();
            return Ok(templates);
        }
    }
}

