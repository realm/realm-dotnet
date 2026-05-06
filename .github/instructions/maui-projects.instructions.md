---
description: "Use when modifying MAUI projects, Tests.Maui, QuickJournal, MAUI csproj files, MauiProgram, MainPage, native references, or package baselines."
applyTo: "Tests/Tests.Maui/**, examples/QuickJournal/**"
---

# MAUI Project Guidelines

- Keep the active target set aligned with MAUI Android, iOS, MacCatalyst, and Windows.
- Active TFM baseline is `net10.0-*`; do not reintroduce `net7.0`, `net6.0`, or `net8.0` into MAUI project files unless a build constraint explicitly requires a temporary fallback.
- Keep package baselines between `Tests/Tests.Maui` and `examples/QuickJournal` strategically aligned.
- Preserve the headless test runner flow in `Tests/Tests.Maui` because CI depends on it.
- When changing native references in `Tests/Tests.Maui`, keep Windows x64, Android ABIs, iOS device or simulator, and MacCatalyst paths consistent with wrapper outputs.
- The SDK is pinned via `global.json` in the repository root; do not hardcode a different SDK version in project files.