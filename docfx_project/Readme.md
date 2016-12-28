# Building API reference docs

We're using [DocFX](https://dotnet.github.io/docfx/index.html) to build API reference docs.

## Getting started

Currently docfx only seems to [work on Windows](https://github.com/docascode/docfx-seed/issues/2).

1. Run `choco install docfx`.
1. Run `choco install nuget.commandline`.

## Building the docs
1. Open cmd in `$SolutionDir\docfx_project` and run `nuget restore -PackagesDirectory ../packages`.
1. Open cmd in `$SolutionDir` and run `docfx docfx_project/docfx.json`.
1. To preview the docs, run `docfx docfx_project/docfx.json --serve`.
