---
description: "Use when modifying GitHub Actions workflows, MAUI jobs, wrapper matrix entries, legacy UWP cleanup, or CI validation for Android, iOS, MacCatalyst, and Windows."
applyTo: ".github/workflows/*.yml"
---

# Workflow Guidelines

- Treat the MAUI jobs in `pr.yml` and `main.yml` as the active platform gate.
- Do not add new UWP jobs or wrapper outputs back into the workflow matrix.
- Keep required wrapper artifacts for Windows x64, Android ABIs, iOS device or simulator, and MacCatalyst intact.
- When removing legacy matrix entries, also remove their cache keys, artifact names, conditions, and downstream references.
- Prefer small symmetric edits across `pr.yml` and `main.yml`.