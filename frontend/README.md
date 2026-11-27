# FeedbackAI Dashboard

React + TypeScript + Vite UI for the FeedbackAI API (`CustomerFeedback.Api`).

## Run

Prerequisites: Node.js 20+, API running at `http://localhost:5047` (see root `README.md`).

```bash
npm install
npm run dev
```

Open `http://localhost:5173`. API traffic goes to `/api` and is proxied to the backend (`vite.config.ts`).

## Build

```bash
npm run build
npm run preview
```

## Environment

Copy `.env.example` only if you need non-default API proxy or base URL settings. **Do not** put Groq keys here—they belong on the server.

## AI analysis

Use **Analyze** on a feedback row. The UI calls `POST /api/feedback/{id}/analyze`; Groq credentials never leave the API process.