using System.Collections.Generic;
using System.Threading.Tasks;
using CVisionAI.Domain.Entities;

namespace CVisionAI.Services
{
    public interface ICvRenderer
    {
        Task<string> RenderHtmlAsync(Profile profile, IEnumerable<Experience> experiences, Template template, string targetRole, string language);
    }
}

