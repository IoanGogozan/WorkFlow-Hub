# Coding Standards

## Purpose

Code must stay easy to read, review, test, and change. Large files slow down development, hide responsibilities, make reviews harder, and increase the risk of accidental regressions.

## File Size Rule

Avoid large source files. If a file grows past the warning limit, consider splitting it. If it reaches the hard limit, it must be split before the feature is accepted unless there is a documented exception.

| File type | Warning limit | Hard limit | Notes |
| --- | ---: | ---: | --- |
| React component `.tsx` | 200 lines | 300 lines | Split into smaller components, hooks, and feature helpers. |
| Next.js route/page/layout `.tsx` | 180 lines | 250 lines | Keep pages mostly composition; move feature logic out. |
| TypeScript utility/service `.ts` | 250 lines | 350 lines | Split by responsibility. |
| React hook `.ts/.tsx` | 120 lines | 200 lines | One main concern per hook. |
| Zod schemas/types `.ts` | 250 lines | 400 lines | Split by feature/domain when needed. |
| C# entity/value object | 150 lines | 250 lines | Keep domain objects focused. |
| C# command/query/handler | 180 lines | 280 lines | Prefer one use case per handler. |
| C# controller/API endpoint file | 200 lines | 300 lines | Use endpoint groups or vertical slices. |
| C# service/adapter | 250 lines | 400 lines | Split integration clients, mapping, and orchestration. |
| EF Core DbContext | 250 lines | 400 lines | Move configurations to separate `IEntityTypeConfiguration` classes. |
| Test file | 300 lines | 500 lines | Split by behavior area, not only by class name. |
| Terraform file | 250 lines | 400 lines | Split by module/resource group. |
| Markdown docs | 500 lines | 800 lines | Split by topic when docs become hard to scan. |

Generated files, lockfiles, migrations, snapshots, and OpenAPI output can exceed these limits, but generated files should be clearly marked and should not contain hand-written business logic.

## Function and Method Size

Use these limits as review guidance:

- Preferred function/method size: under 40 lines.
- Warning limit: 60 lines.
- Hard limit: 100 lines.

Exceptions are allowed for simple declarative mapping or test setup, but complex logic should be split into named functions.

## Component Rules

Frontend components should follow these rules:

- One component should have one clear UI responsibility.
- Page components should compose feature components, not contain business logic.
- Data loading, form schemas, formatting, and API calls should live outside large JSX files.
- Repeated UI patterns should become small local components before they are reused globally.
- Avoid deeply nested JSX that makes the component hard to scan.

## Backend Rules

Backend code should follow these rules:

- Keep API endpoints thin.
- Keep one command/query handler focused on one use case.
- Put validation in validators or request models, not mixed into controllers.
- Put external API details in infrastructure adapters.
- Put tenant and authorization checks near the application boundary.
- Split EF Core entity configuration into separate files when the DbContext grows.

## Test Rules

Tests should be readable and specific:

- Test file names should describe the behavior area.
- Split large test files by feature, role, or failure mode.
- Use test data builders/fixtures when setup becomes noisy.
- Negative tests should be explicit and named after the rejected behavior.

## Review Rule

When a file crosses a warning limit, the pull request or implementation note should explain why it remains readable. When a file crosses a hard limit, split it before accepting the feature or document a temporary exception with a follow-up backlog item.

## CI Direction

Add automated file-size checks once the codebase is scaffolded. The first version can be a simple script that fails CI for hand-written source files over hard limits while ignoring generated files, migrations, lockfiles, and build output.
