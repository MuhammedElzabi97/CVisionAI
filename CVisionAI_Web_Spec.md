# CVision AI — Web Frontend (React.js) Spec (Cursor-ready)

> Goal: Build the **web app** for an AI-powered, ATS-friendly CV builder matching the reference UI flow:
> **Dashboard → Experience Optimizer → Template Gallery → Preview & Export (+ ATS Report)**

---

## 0) Tech Stack & Constraints

- **React.js** + **TypeScript** (recommended)
- Build tool: **Vite** (fast) or CRA
- Router: `react-router-dom`
- UI: TailwindCSS or MUI (pick one; MVP: Tailwind + simple components)
- API: consumes **ASP.NET Web API** (REST)
- Preview: render **server-provided HTML** safely (iframe or sanitized container)
- Export: backend returns **PDF/DOCX download URL**

**MVP scope (1–2 days):**
- Auth (optional / can be mocked)
- Profile builder (minimal wizard)
- AI optimize experience text
- Template selection (3 templates)
- Preview (HTML) + ATS report + export buttons

---

## 1) Pages (Routes) — Mirrors the UI

### A) `/dashboard` — `DashboardPage`
**UI blocks**
- Header: “Welcome back, {name}”
- Card: **Latest ATS Score**
  - Score (0–100)
  - Mini metrics: Keyword Match, Formatting, Readability
  - Button: **Create New CV**
- Section: **AI Insights**
  - Insight cards (skill gap, quantify impact, etc.)
- Section: **Your Resumes**
  - List of resumes (title + last update + actions)
  - Actions: Preview / Duplicate / Delete (delete optional)

**Actions**
- Create New CV → `/builder/profile` (or `/builder/experience` if profile exists)
- Resume item Preview → `/cv/:cvId/preview`

---

### B) `/builder/experience` — `ExperienceOptimizerPage`
**Purpose:** Add/edit a work experience entry and press **AI Optimize**.

**UI blocks**
- Stepper/progress: “Step 3 of 5 – Completion %”
- Form:
  - Job Title
  - Company Name
  - Start Date / End Date
  - Description (textarea)
  - Button: **AI Optimize**
- Bottom buttons:
  - Save Draft
  - Continue → Template Gallery

**Actions**
- AI Optimize → call backend rewrite endpoint
- Save Draft → save profile/experience
- Continue → `/builder/templates`

---

### C) `/builder/templates` — `TemplateGalleryPage`
**UI blocks**
- Tabs/filters: “ATS Minimal”, “Creative” (MVP: 2 categories)
- Grid of templates (3–4)
  - Badge: “95% ATS”
  - Title + subtitle
  - Selected state
- CTA: **Use '{templateName}' Template**

**Actions**
- Select template → store `templateId`
- Use template → generate preview (API) → `/builder/preview`

---

### D) `/builder/preview` — `PreviewExportPage`
**UI blocks**
- Header: Preview & Export
- **CV Preview**
  - HTML preview (iframe or div with sanitized HTML)
- **ATS Compatibility Report**
  - Overall compatibility %
  - Missing keywords list with impact chip (High/Medium)
- Export buttons:
  - Download PDF
  - Download DOCX

**Actions**
- Download buttons → call export endpoints → redirect/open file url

---

### Optional: `/builder/profile` — `ProfileWizardPage`
**Purpose:** Quick profile intake if you want a clean MVP.
- Personal info (name, email, location)
- Skills (tags)
- Education (1 entry)
- Projects (1–2 entries)
- Target role + language

Then continue to experience optimizer.

---

## 2) Component Breakdown (Reusable)

### `AtsScoreCard`
- Props: `overallScore, keywordMatch, formatting, readability, onCreateNew`

### `InsightCard`
- Props: `title, description, type`

### `ResumeListItem`
- Props: `title, updatedAt, onPreview`

### `TemplateCard`
- Props: `template, selected, onSelect`

### `CvPreviewFrame`
- Props: `html`
- Implementation options:
  1) `iframe` with `srcDoc={html}` (recommended for isolation)
  2) Sanitized `dangerouslySetInnerHTML` (only if you sanitize)

### `AtsReportPanel`
- Props: `report: AtsReport`

### `PrimaryButton`, `SecondaryButton`, `Chip`, `Tabs`

---

## 3) State & Data Flow

**Core entities**
- `profileId`
- `selectedTemplateId`
- `generatedCv` (includes `htmlPreview` + `atsReport`)

**Flow**
1. User fills profile + experience → save profile
2. User hits AI optimize → update experience description
3. User selects template → call generate endpoint
4. Preview & export uses generatedCv

**Store**
- Small: `zustand` or React Context for `profileId`, `templateId`, `generatedCvId`

---

## 4) Types (Frontend)

```ts
export type Template = {
  id: string;
  name: string;
  category: "ATS_MINIMAL" | "CREATIVE" | "ACADEMIC";
  atsScoreHint: number;
  subtitle?: string;
};

export type AtsReport = {
  overallScore: number;
  keywordMatch: number;
  formatting: number;
  readability: number;
  missingKeywords: { keyword: string; impact: "HIGH" | "MEDIUM" | "LOW" }[];
  notes?: string[];
};

export type GeneratedCv = {
  id: string;
  title: string;
  updatedAt: string;
  templateId: string;
  htmlPreview: string;
  atsReport: AtsReport;
  files?: { pdfUrl?: string; docxUrl?: string };
};
```

---

## 5) Backend API Contract (ASP.NET) — Web Consumes These

### Profile
- `GET /api/profiles/me`
- `POST /api/profiles`
- `PUT /api/profiles/{id}`

### AI Optimize (Experience)
- `POST /api/ai/optimize/experience`
  - request:
    ```json
    {
      "profileId": "string",
      "experienceId": "string",
      "targetRole": "Backend Intern",
      "jobDescriptionText": "optional",
      "language": "EN"
    }
    ```
  - response:
    ```json
    { "optimizedDescription": "string", "suggestedBullets": ["..."] }
    ```

### Templates
- `GET /api/templates`

### Generate Preview
- `POST /api/cv/generate`
  - request:
    ```json
    { "profileId":"string", "templateId":"string", "targetRole":"string", "language":"EN", "jobDescriptionText":"optional" }
    ```
  - response: `GeneratedCv`

### Export
- `POST /api/cv/{cvId}/export/pdf` → `{ "url":"https://..." }`
- `POST /api/cv/{cvId}/export/docx` → `{ "url":"https://..." }`

---

## 6) Project Structure (Suggested)

```
src/
  app/
    router.tsx
    layout/
      AppShell.tsx
  pages/
    DashboardPage.tsx
    ProfileWizardPage.tsx
    ExperienceOptimizerPage.tsx
    TemplateGalleryPage.tsx
    PreviewExportPage.tsx
  components/
    AtsScoreCard.tsx
    InsightCard.tsx
    ResumeListItem.tsx
    TemplateCard.tsx
    CvPreviewFrame.tsx
    AtsReportPanel.tsx
    ui/
      Button.tsx
      Chip.tsx
      Tabs.tsx
  services/
    api.ts
    profileService.ts
    aiService.ts
    templateService.ts
    cvService.ts
  store/
    useCvStore.ts
  types/
    cvTypes.ts
  utils/
    dates.ts
```

---

## 7) Security Notes (HTML Preview)

**Preferred:** `iframe srcDoc` (prevents CSS/JS leaking into app)
- Ensure backend **does not include scripts** in HTML
- If you must render in a div, sanitize HTML (DOMPurify)

---

## 8) MVP Acceptance Criteria (Done Definition)

- [ ] Dashboard shows ATS score + resume list (mock ok)
- [ ] Experience page: AI Optimize updates description
- [ ] Template gallery: select + generate preview
- [ ] Preview page renders HTML + ATS report
- [ ] PDF/DOCX download works

---

## 9) Cursor Tasks (Copy/Paste)

### Task 1 — Setup
- Create Vite React TS project, install router, Tailwind/MUI, create base layout.

### Task 2 — Pages + Routing
- Implement 4 pages and transitions:
  Dashboard → Experience → Templates → Preview

### Task 3 — API Layer
- Implement api.ts + service files (profile/ai/templates/cv).

### Task 4 — HTML Preview
- Use iframe srcDoc + responsive container.

### Task 5 — Export
- Call export endpoints, open returned URL in new tab.

---

## 10) Notes for Backend Team (Smooth Web Preview)

- Return `htmlPreview` as **self-contained HTML** with inline CSS
- Export endpoints return URLs with correct headers for download
- Enable CORS for the web domain (dev: localhost)

---

If you want, I can also generate:
- a **Vite + Tailwind starter skeleton** (pages/components/services),
- or a **UI style guide** to match the screenshot (spacing, card styles, badges).
