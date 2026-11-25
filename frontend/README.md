# Customer Feedback Dashboard

React + TypeScript + Vite UI for the Customer Feedback API.

## Prerequisites

- Node.js 20+
- API running at `http://localhost:5047` (see repository root `README.md`)

## Development

```bash
npm install
npm run dev
```

Open `http://localhost:5173`. API requests go to `/api` and are proxied to the backend (see `vite.config.ts`).

Copy `.env.example` to `.env` only if you need to override proxy or API base URL settings.

## Build

```bash
npm run build
npm run preview
```

`preview` also proxies `/api` to the local API.

## AI analysis

Use **Analyze** on a feedback row. The browser calls the API only; Groq credentials stay on the server (`Groq:ApiKey`). See the repository root `README.md` for configuration.