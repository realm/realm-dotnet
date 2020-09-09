# Building API reference docs

We're using [DocFX](https://dotnet.github.io/docfx/index.html) to build API reference docs.

## Getting started

Docfx works best on Windows when run from Developer Command Prompt.

1. Run `choco install docfx`.

## Building the docs
1. In `$SolutionDir` run `docfx Docs/docfx.json`.
1. To preview the docs, run `docfx Docs/docfx.json --serve`. To stop serving the docs, type `exit` and press enter (`Ctr+C` doesn't work).