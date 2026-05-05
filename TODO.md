# TODO

## Phase 0 - Scope

- [x] MAUI Android, iOS, MacCatalyst und Windows als aktive Zielplattformen festschreiben.
- [x] UWP, Xamarin, Avalonia und Sync-bezogene Weiterentwicklung aus dem aktiven Scope nehmen.
- [x] `net6.0` aus `Realm/Realm/Realm.csproj` entfernt.
- [ ] `.NET 10` als aktive Basis festlegen: `Realm/Realm/Realm.csproj`, `Tests/Tests.Maui/Tests.Maui.csproj` und die MAUI-Pipeline auf `net10.0` bzw. `net10.0-*` anheben; `net8.0` nur temporaer behalten, falls die Umstellung das kurzfristig erzwingt.
- [ ] `.NET 10` SDK-, MAUI-Workload- und CI-Voraussetzungen festziehen.

## Phase 1 - Leitplanken

- [x] Projektweite Leitplanken fuer MAUI-only Entwicklung in `.github/` angelegt.
- [x] Dateispezifische Instructions fuer MAUI-Projekte und Workflows angelegt.
- [ ] Optional ein wiederverwendbares Skill fuer MAUI-Wartungsaufgaben definieren.

## Phase 2 - Projektbaseline

- [ ] `Tests/Tests.Maui/Tests.Maui.csproj` von `net8.0-*` auf `net10.0-*` anheben und als Referenzprojekt fuer aktive Plattformen und Native References verwenden.
- [x] `examples/QuickJournal/QuickJournal.csproj` von `net7.0` auf die Zwischenbasis angehoben.
- [ ] `examples/QuickJournal/QuickJournal.csproj` von `net8.0-*` auf `net10.0-*` anheben.
- [x] `examples/QuickJournal/QuickJournal.csproj` von `Realm` 11.x auf die aktuelle Weiterentwicklung umgestellt.
- [ ] Paketstaende zwischen `Tests/Tests.Maui`, `examples/QuickJournal` und `.NET 10`-kompatiblen MAUI-/Logging-Paketen harmonisieren.
- [ ] Build-relevante Nebenprojekte wie `Tests/Benchmarks/PerformanceTests/PerformanceTests.csproj` auf `.NET 10` pruefen und anheben.
- [ ] `examples/QuickJournal/README.md` auf veraltete MAUI-Hinweise pruefen und anpassen.

## Phase 3 - Laufzeit und Storage

- [ ] UWP-spezifische Ordnerermittlung in `Realm/Realm/InteropConfig.cs` entfernen oder sauber isolieren.
- [ ] Auswirkungen der Storage-Aenderung in `Realm/Realm/Configurations/RealmConfigurationBase.cs` pruefen.
- [ ] Aufrufer von `AddPotentialStorageFolder`, `SetDefaultStorageFolder` und `GetDefaultStorageFolder` auf MAUI-Relevanz pruefen.
- [ ] Defaultpfade auf Android, iOS und Windows verifizieren.

## Phase 4 - CI und Wrapper

- [x] `test-uwp` aus `.github/workflows/pr.yml` entfernt.
- [x] `test-uwp` aus `.github/workflows/main.yml` entfernt.
- [x] UWP-Wrapper-Artefakte und Buildpfade aus `.github/workflows/wrappers.yml` entfernt.
- [ ] `.github/pkl-workflows` und generierte Workflows von `net8.0-*` auf `net10.0-*` umstellen.
- [ ] Windows-x64-, Android-, iOS- und Catalyst-Wrapperpfade gegen `Tests/Tests.Maui/Tests.Maui.csproj` abgleichen.
- [ ] Schnelle Core-Regressionen definieren, die zusaetzlich zur MAUI-Matrix bestehen bleiben.

## Phase 5 - Tests

- [ ] MAUI-spezifische Regressionstests fuer Storage, Native Loading und Headless-Runs ergaenzen.
- [ ] Gemeinsame Tests in `Tests/Realm.Tests` auf Legacy-Annahmen pruefen.
- [ ] `examples/QuickJournal` als Smoke-Test-App in die Verifikation aufnehmen.

## Phase 6 - Dokumentation

- [ ] `README.md` auf MAUI als primaeren Entwicklungs- und Nutzungspfad ausrichten.
- [ ] Guides und Build-Hinweise von Xamarin-, UWP- und Sync-Altlasten bereinigen.
- [ ] Support-Matrix sowie `.NET 10`- und MAUI-Voraussetzungen dokumentieren.

## Phase 7 - Nachgelagertes Aufraeumen

- [ ] Weitere Altlasten erst nach stabilem MAUI-Baseline-Stand priorisieren.
- [ ] Fody- und Source-Generator-Themen getrennt von der MAUI-Umstellung behandeln.
- [ ] Groessere Dependency-Upgrades erst nach gruener MAUI-Pipeline angehen.
