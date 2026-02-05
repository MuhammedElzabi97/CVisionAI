# CVision AI — Backend (ASP.NET Core .NET 8) Spec (Cursor-ready)

> Goal: Build the **ASP.NET Core Web API** backend for an AI-powered, ATS-friendly CV builder:
> **Profile intake → AI optimize sections → Generate role-based CV → HTML preview → Export PDF/DOCX → ATS report**

This spec is written so Cursor can implement it quickly.

---

## 0) Tech Stack & Decisions

- **.NET 8** — ASP.NET Core Web API
- Architecture: **Clean-ish** (Api / Application / Domain / Infrastructure) *or* simple layered (Controllers/Services/Repositories)
- ORM: **EF Core**
- DB: **PostgreSQL** (recommended) or SQL Server
- Auth: JWT (optional for MVP; can be stubbed)
- AI: OpenAI / Azure OpenAI (via HttpClient)
- Export:
  - Option A (fast): **QuestPDF** for PDF + docx via **Open XML** or DocX
  - Option B (good HTML fidelity): HTML → PDF using a converter (e.g., Playwright/Puppeteer service) *(more work)*
- Return **HTML preview** + **ATS report** in the generate response.

**MVP scope (1–2 days):**
- CRUD profile (minimal)
- Optimize experience section (AI)
- Templates list
- Generate CV (HTML + ATS report)
- Export PDF/DOCX as downloadable URLs

---

## 1) Domain Model (Entities)

### `User` (optional for MVP)
- `Id (Guid)`
- `Email`
- `PasswordHash` (if local auth)
- `CreatedAt`

### `Profile`
- `Id (Guid)`
- `UserId (Guid?)`
- `FullName`
- `Email?`
- `Phone?`
- `Location?`
- `LinksJson?` (store JSON string)
- `Summary?`
- `SkillsJson` (JSON array string)
- `TargetRole`
- `Language` ("TR" | "EN")
- `CreatedAt`, `UpdatedAt`

### `Experience`
- `Id (Guid)`
- `ProfileId (Guid)`
- `JobTitle`
- `Company`
- `StartDate (DateOnly or DateTime)`
- `EndDate (DateOnly?)` (null => Present)
- `Description`
- `CreatedAt`, `UpdatedAt`

### `Education` (optional MVP)
- `Id`, `ProfileId`, `School`, `Field?`, `Degree?`, `StartDate?`, `EndDate?`

### `Project` (optional MVP)
- `Id`, `ProfileId`, `Name`, `TechJson?`, `Description`, `Link?`

### `Template`
- `Id (Guid)`
- `Name`
- `Category` ("ATS_MINIMAL" | "CREATIVE" | "ACADEMIC")
- `AtsScoreHint (int 0-100)`
- `Subtitle?`
- `HtmlLayoutKey` (string key to choose layout)
- Seed 3 templates

### `GeneratedCv`
- `Id (Guid)`
- `ProfileId`
- `TemplateId`
- `Title`
- `TargetRole`
- `Language`
- `HtmlPreview` (string) *(or store path)*
- `AtsReportJson` (string)
- `CreatedAt`, `UpdatedAt`

### `GeneratedFile`
- `Id (Guid)`
- `GeneratedCvId`
- `FileType` ("PDF" | "DOCX")
- `FilePath` (or blob key)
- `PublicUrl` (optional)
- `CreatedAt`

---

## 2) DTOs (Request/Response Contracts)

### Profile
```json
// POST /api/profiles
{
  "fullName": "Alex Johnson",
  "email": "alex@mail.com",
  "phone": "+90...",
  "location": "Istanbul",
  "links": [{"label":"GitHub","url":"https://..."}],
  "summary": "Short summary",
  "skills": ["ASP.NET", "React", "SQL"],
  "targetRole": "Backend Intern",
  "language": "EN"
}
```

### Experience
```json
// POST /api/profiles/{profileId}/experiences
{
  "jobTitle": "Junior Developer",
  "company": "TechNova",
  "startDate": "2024-01-01",
  "endDate": null,
  "description": "Built REST APIs..."
}
```

### AI Optimize Experience
```json
// POST /api/ai/optimize/experience
{
  "profileId": "guid",
  "experienceId": "guid",
  "targetRole": "Backend Intern",
  "jobDescriptionText": "optional job post text",
  "language": "EN"
}
```
Response:
```json
{
  "optimizedDescription": "ATS-friendly description...",
  "suggestedBullets": ["Improved ...", "Implemented ..."]
}
```

### Generate CV
```json
// POST /api/cv/generate
{
  "profileId": "guid",
  "templateId": "guid",
  "targetRole": "Backend Intern",
  "language": "EN",
  "jobDescriptionText": "optional"
}
```

Response (core):
```json
{
  "id": "guid",
  "title": "Backend Intern CV",
  "updatedAt": "2026-02-04T12:00:00Z",
  "templateId": "guid",
  "htmlPreview": "<html>...</html>",
  "atsReport": {
    "overallScore": 85,
    "keywordMatch": 92,
    "formatting": 88,
    "readability": 74,
    "missingKeywords": [
      {"keyword":"Unit Testing","impact":"MEDIUM"},
      {"keyword":"CI/CD","impact":"HIGH"}
    ],
    "notes": ["Use more quantified impact statements."]
  },
  "files": {
    "pdfUrl": null,
    "docxUrl": null
  }
}
```

### Export
```json
// POST /api/cv/{cvId}/export/pdf
{ "url": "https://..." }
```

---

## 3) API Endpoints (Minimal)

### Templates
- `GET /api/templates`
  - Returns seeded template list

### Profiles
- `GET /api/profiles/{id}`
- `GET /api/profiles/me` *(if auth)*
- `POST /api/profiles` *(create)*
- `PUT /api/profiles/{id}` *(update)*

### Experiences
- `GET /api/profiles/{profileId}/experiences`
- `POST /api/profiles/{profileId}/experiences`
- `PUT /api/experiences/{experienceId}`
- `DELETE /api/experiences/{experienceId}`

### AI
- `POST /api/ai/optimize/experience`
- (optional) `POST /api/ai/suggest/skills`
- (optional) `POST /api/ai/generate/summary`

### CV Generation + Export
- `POST /api/cv/generate`
- `GET /api/cv/{cvId}`
- `POST /api/cv/{cvId}/export/pdf`
- `POST /api/cv/{cvId}/export/docx`

### Files (optional)
- `GET /files/{fileId}` or serve from static folder

---

## 4) Services (Business Logic)

### `IAiWriterService`
Responsibilities:
- Build prompt with:
  - target role
  - language
  - existing experience text
  - optional job description text
  - ATS rules
- Return optimized text + bullet suggestions

**Prompt rules (ATS-friendly)**
- Use standard headings: Summary, Skills, Experience, Projects, Education
- Prefer action verbs + metrics (%, ms, users, costs)
- Avoid emojis, tables, 2-column layout (for ATS Minimal)

### `IAtsScoringService` (Rule-based MVP)
Compute:
- `keywordMatch`: based on intersection of jobDescription keywords vs generated content
- `formatting`: penalize tables/columns, too many icons, missing headings
- `readability`: simple heuristic (avg sentence length / bullet ratio)

Return:
- overallScore = weighted average (e.g., 0.5 keyword + 0.3 formatting + 0.2 readability)
- missingKeywords: top N keywords not found

### `ICvRenderer`
Input: Profile + Experiences + Template layout key  
Output: self-contained **HTML string** (inline CSS)
- Template 1: ATS Minimal (single column)
- Template 2: Modern Tech (still ATS-safe)
- Template 3: Academic/Intern (projects highlight)

### `IExportService`
- Generate PDF from HTML or directly from model
- Generate DOCX from model (OpenXML) or prebuilt doc template
- Save file to `/storage` and return URL

---

## 5) Storage & File URLs

**Simple MVP approach**
- Save exports to: `./storage/{cvId}/{timestamp}.pdf`
- Expose static files:
  - `app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(storagePath), RequestPath="/storage" })`
- Return URLs like:
  - `/storage/{cvId}/cv.pdf`

---

## 6) Project Structure (Recommended)

```
src/
  CVisionAI.Api/
    Controllers/
      TemplatesController.cs
      ProfilesController.cs
      ExperiencesController.cs
      AiController.cs
      CvController.cs
    Program.cs
    appsettings.json
  CVisionAI.Application/
    DTOs/
    Services/
      AiWriterService.cs
      AtsScoringService.cs
      CvRenderer.cs
      ExportService.cs
  CVisionAI.Domain/
    Entities/
    Enums/
  CVisionAI.Infrastructure/
    Persistence/
      AppDbContext.cs
      Migrations/
    External/
      OpenAiClient.cs
    Storage/
      FileStorage.cs
```

*(For fastest MVP you can keep it all inside Api project, but keep folders.)*

---

## 7) Seed Data (Templates)

Seed 3 templates on startup/migration:
1. **ATS Minimal** — category ATS_MINIMAL, atsScoreHint 95, layoutKey `ats_minimal`
2. **Modern Tech** — category CREATIVE, atsScoreHint 90, layoutKey `modern_tech`
3. **Academic Classic** — category ACADEMIC, atsScoreHint 92, layoutKey `academic_classic`

---

## 8) CORS & Environment

- Dev origins:
  - React: `http://localhost:5173`
  - React Native: Metro / Expo (set accordingly)
- Add CORS policy allowing dev origins.
- Use `IHttpClientFactory` for AI calls.

---

## 9) Security & Safety Notes

- Never log full CV content + job description in production logs
- Add basic request size limits (jobDescription can be large)
- Rate limit AI endpoints (even simple in-memory)

---

## 10) MVP Acceptance Criteria (Done Definition)

- [ ] `GET /api/templates` returns 3 templates
- [ ] Create profile + add experience works (EF Core)
- [ ] `POST /api/ai/optimize/experience` returns rewritten description
- [ ] `POST /api/cv/generate` returns HTML + ATS report
- [ ] Export endpoints create PDF/DOCX and return downloadable URL
- [ ] Swagger docs are clean and testable end-to-end

---

## 11) Cursor Tasks (Copy/Paste)

### Task 1 — Setup
- Create .NET 8 Web API, add EF Core + Postgres provider, create DbContext, migrations.

### Task 2 — Entities + CRUD
- Implement Profile + Experience entities and controllers.

### Task 3 — Templates Seed
- Implement Template entity and seeding, return via endpoint.

### Task 4 — AI Optimize
- Implement AiWriterService with HttpClient and endpoint.

### Task 5 — Generate CV
- Build CvRenderer to return HTML + AtsScoringService to return report.

### Task 6 — Export
- Implement ExportService to create PDF/DOCX and serve from /storage.

---

## 12) Minimal HTML Template Guidance (Renderer)

- Inline CSS only
- No JS
- Headings: `<h1>`, `<h2>`
- Use `<ul><li>` for bullets
- Dates formatted consistently: `MMM yyyy`

Example skeleton:
```html
<html>
  <head>
    <meta charset="utf-8"/>
    <style>
      body { font-family: Arial; margin: 32px; }
      h1 { margin-bottom: 4px; }
      h2 { margin-top: 16px; font-size: 14px; text-transform: uppercase; }
      .muted { color: #666; }
    </style>
  </head>
  <body>
    <h1>Full Name</h1>
    <div class="muted">Istanbul • email • phone • link</div>
    <h2>Summary</h2>
    <p>...</p>
    <h2>Skills</h2>
    <p>ASP.NET, React, SQL</p>
    <h2>Experience</h2>
    <ul><li>...</li></ul>
  </body>
</html>
```

---

If you want next:
- I can generate **actual C# code skeletons** (Entities, Controllers, DTOs, Services),
- plus `appsettings.json` and CORS config.
