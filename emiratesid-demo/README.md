# Emirates ID Extractor Demo

A modern web application for extracting structured data from UAE Emirates ID cards using Azure AI Document Intelligence.

## Tech Stack

- **Framework**: SvelteKit 2 with Svelte 5 (runes mode)
- **Styling**: Tailwind CSS 4
- **Language**: TypeScript
- **Testing**: Playwright (E2E)

## Features

- Upload front and back images of Emirates ID
- Upload PDF documents containing ID scans
- Real-time extraction using Azure AI Document Intelligence
- Responsive design for mobile and desktop
- Accessibility compliant (keyboard navigation, screen reader support)

## Getting Started

### Prerequisites

- Node.js 18+
- Backend API running (see [Backend Setup](#backend-setup))

### Installation

```bash
npm install
```

### Development

```bash
npm run dev -- --host
```

The app will be available at `http://localhost:5173`

### Backend Setup

The frontend requires the .NET backend API to be running:

```bash
cd ../dotNET/Brimit.EmiratesIDExtractor
dotnet run --project Brimit.EmiratesIDExtractor
```

Backend runs on `https://localhost:44357`

## Project Structure

```
emiratesid-demo/
├── src/
│   ├── lib/
│   │   ├── api/          # API client for backend communication
│   │   ├── components/   # Reusable Svelte components
│   │   │   ├── ErrorAlert.svelte
│   │   │   ├── FilePreview.svelte
│   │   │   ├── IDCardFlip.svelte
│   │   │   ├── IDCardSilhouette.svelte
│   │   │   ├── ResultCard.svelte
│   │   │   ├── Skeleton.svelte
│   │   │   ├── StepIndicator.svelte
│   │   │   └── UploadZone.svelte
│   │   └── utils/        # Utility functions (validation, cn)
│   ├── routes/
│   │   ├── +layout.svelte  # Main layout with header/footer
│   │   └── +page.svelte    # Home page with upload flow
│   ├── app.css           # Global styles and Tailwind config
│   └── app.html          # HTML template
├── static/               # Static assets (favicon, images)
├── e2e/                  # Playwright E2E tests
└── playwright.config.ts  # Playwright configuration
```

## API Endpoints

The frontend communicates with these backend endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/id-document/parse/uae-id` | Parse Emirates ID from images |
| POST | `/api/id-document/parse/uae-id/pdf` | Parse Emirates ID from PDF |

## Testing

```bash
# Run all E2E tests
npx playwright test

# Run specific browser
npx playwright test --project=chromium

# Run with UI
npx playwright test --ui
```

## Building for Production

```bash
npm run build
npm run preview
```

## License

For demonstration purposes only.
