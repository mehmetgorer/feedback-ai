---
name: code-review
description: >-
  Performs read-only reviews of local changes or the codebase: inspects Git
  diffs, checks security and business logic, flags missing tests, and reports
  findings by severity with file references. Use when the user asks for a code
  review, PR review, security review, or implementation audit.
---

# Code review (read-only)

Review work in this repository without editing files. The deliverable is a structured findings report only.

## Constraints

- **Never modify code** during review. Do not create commits, fix issues, or refactor unless the user explicitly switches to implementation work afterward.
- Use read-only inspection: read files, search the tree, and run non-mutating Git commands (`git status`, `git diff`, `git log`).
- Verify claims against **actual source**, not assumptions.

## Workflow

### 1. Establish scope

- If the user names a branch, commit range, or PR, review **that delta**.
- Otherwise, run from the repo root:
  - `git status` — untracked and modified files
  - `git diff` — unstaged changes
  - `git diff --staged` — staged changes
  - `git log -5 --oneline` — recent context (optional)
- When there is no Git delta, review the files or areas the user specified, or the components touched by the task.

### 2. Read related code

- Open changed files and their callers (controllers → services → data, validators → DTOs).
- Note existing patterns in [`AGENTS.md`](../../AGENTS.md) (layering, test layout, naming).

### 3. Review dimensions

| Dimension | What to check |
|-----------|----------------|
| **Security** | Missing auth on sensitive data, injection, unsafe logging of PII/secrets, weak validation, permissive config (`AllowedHosts`, CORS), dependency advisories (`dotnet list package --vulnerable` when packages changed). |
| **Business logic** | CRUD and domain rules match requirements; edge cases (not found, pagination, timestamps, idempotency); consistency between validators, services, and EF config. |
| **Tests** | New or changed behavior has unit/integration coverage; tests are isolated (shared fixtures, polluted DB state); naming follows `Operation_Condition_Outcome`. |
| **API contract** | Status codes, validation errors, breaking DTO or route changes. |

### 4. Report format

Prioritize findings **Critical → High → Medium → Low → Informational**.

For each finding include:

1. **Severity** and short title
2. **Affected files** — repo-relative paths (and line ranges when helpful)
3. **Evidence** — what the code does today (brief quote or behavior)
4. **Impact** — why it matters
5. **Recommendation** — optional, one or two sentences (no code changes in this skill)

End with:

- **Summary** — count by severity and overall risk
- **Strengths** — what looks solid (brief)
- **Unresolved / out of scope** — items not verified or deferred

### Severity guide

| Level | Examples |
|-------|----------|
| Critical | Exploitable auth bypass, secret exposure, data loss in normal use |
| High | Unauthenticated PII access, confirmed high-severity CVEs in new deps, wrong business outcome |
| Medium | Missing validation paths, flaky or coupled tests, operational misconfig |
| Low | Style, duplication, minor edge cases |
| Informational | Positive notes, plan-aligned omissions |

### 5. Close

State explicitly that the review was **read-only** and no files were modified.

## Project-specific pointers

- API: `CustomerFeedback.Api/Controllers/`, `Services/`, `Validation/`, `Data/`
- Tests: `CustomerFeedback.Tests/Integration/`, `Validation/`, `Infrastructure/FeedbackApiFactory.cs`
- Dependencies: `CustomerFeedback.Api/CustomerFeedback.Api.csproj`