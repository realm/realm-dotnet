# Building API reference docs

We're using [DocFX](https://dotnet.github.io/docfx/index.html) to build API reference docs.

## Getting started

Currently docfx only seems to [work on Windows](https://github.com/docascode/docfx-seed/issues/2).

1. Run `choco install docfx --version 2.21.1`. This is the latest version we've tried that works.
1. Run `choco install nuget.commandline`.
1. Make sure you have [Visual Studio 2015](https://www.visualstudio.com/vs/) or [Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159) installed.

## Building the docs
1. Open cmd in `$SolutionDir\Docs` and run `nuget restore -PackagesDirectory ../packages`.
1. Open cmd in `$SolutionDir` and run `docfx Docs/docfx.json`.
1. To preview the docs, run `docfx Docs/docfx.json --serve`.

**Note**: if you have the .NET Core 2.0 SDK installed, you may need to run the `docfx` commands from `Developer Command Prompt for VS 2017`. 