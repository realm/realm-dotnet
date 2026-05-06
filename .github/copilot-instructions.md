# Project Guidelines

## Scope

- Treat MAUI on Android, iOS, MacCatalyst, and Windows as the active product direction for this repository.
- Prefer changes that strengthen `Tests/Tests.Maui` and `examples/QuickJournal` over legacy platform-specific paths.
- Treat UWP, Xamarin, Avalonia, and Atlas Device Sync as legacy or out-of-scope unless a task explicitly targets compatibility cleanup.

## Architecture

- `Realm/Realm` is the shipping SDK surface.
- `Tests/Realm.Tests` provides shared regression coverage.
- `Tests/Tests.Maui` is the active platform validation harness for MAUI targets.
- Native wrapper outputs used by `Tests/Tests.Maui` are part of the active path and should stay aligned with the MAUI project file.

## Change Strategy

- Remove or isolate UWP-specific code paths instead of extending them.
- Keep `netstandard2.0` only where packaging or tooling still requires it; do not add new feature work around deprecated targets.
- Active TFM baseline is `.NET 10` (`net10.0` / `net10.0-*`). `net8.0` is retained temporarily where tooling constraints require it; new work targets `net10.0`.
- The repository SDK is pinned to `.NET 10` via `global.json` in the repository root.
- When updating MAUI examples, keep `QuickJournal` on the same strategic MAUI and Realm baseline as the rest of the repository.
- When editing CI, preserve the MAUI jobs in `.github/workflows/pr.yml` and `.github/workflows/main.yml` and keep the required wrapper outputs in `.github/workflows/wrappers.yml` for Android, iOS, MacCatalyst, and Windows.

## Build and Test

- Prefer validation through `Tests/Tests.Maui` for platform behavior and `Tests/Realm.Tests` for shared regressions.
- Keep workflow changes symmetric between `pr.yml` and `main.yml` unless there is a platform-specific reason not to.

## Skills

- Use the `realm-maui` skill for MAUI maintenance tasks such as TFM upgrades, native reference alignment, package baseline harmonization, CI matrix changes, and headless test runner adjustments.