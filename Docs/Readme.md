# Building API reference docs

We're using [DocFX](https://dotnet.github.io/docfx/index.html) to build API reference docs.

## Getting started

Docfx works best on mac as getting it to run on Windows requires fiddling with the MSBuild paths.

1. Run `brew install docfx`.

## Building the docs
1. Open cmd in `$SolutionDir` and run `docfx Docs/docfx.json`.
1. To preview the docs, run `docfx Docs/docfx.json --serve`. To stop serving the docs, type `exit` and press enter (`Ctr+C` doesn't work).