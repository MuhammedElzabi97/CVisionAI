using CVisionAI.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CVisionAI.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<GeneratedCv> GeneratedCvs { get; set; }
        public DbSet<GeneratedFile> GeneratedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Profile>()
                .HasMany(p => p.Experiences)
                .WithOne(e => e.Profile)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GeneratedCv>()
                .HasOne(g => g.Profile)
                .WithMany()
                .HasForeignKey(g => g.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed minimal templates for MVP
            builder.Entity<Template>().HasData(
                new Template
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "ATS Minimal",
                    Category = "ATS_MINIMAL",
                    AtsScoreHint = 95,
                    Subtitle = "Single-column, ATS-friendly layout",
                    HtmlLayoutKey = "ats_minimal"
                },
                new Template
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Modern Tech",
                    Category = "CREATIVE",
                    AtsScoreHint = 90,
                    Subtitle = "Clean layout suitable for tech roles",
                    HtmlLayoutKey = "modern_tech"
                },
                new Template
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Academic Classic",
                    Category = "ACADEMIC",
                    AtsScoreHint = 92,
                    Subtitle = "Emphasizes education and projects",
                    HtmlLayoutKey = "academic_classic"
                });
        }
    }
}
