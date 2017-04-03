# Building API reference docs

We're using [DocFX](https://dotnet.github.io/docfx/index.html) to build API reference docs.

## Getting started

Currently docfx only seems to [work on Windows](https://github.com/docascode/docfx-seed/issues/2).

1. Run `choco install docfx`.
1. Run `choco install nuget.commandline`.
1. Make sure you have [Visual Studio 2015](https://www.visualstudio.com/vs/) or [Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159) installed.

## Building the docs
1. Open cmd in `$SolutionDir\Docs` and run `nuget restore -PackagesDirectory ../packages`.
1. Rename `Realm\Realm{Sync}\project.json` to something.
1. Rename `Realm\Realm{Sync}\packages.Docs.config` to `Realm\Realm{Sync}\packages.config`.
1. Open `Realm.Docs.sln` and make sure all files are included in the docs projects as well as that the solution builds.
1. Open cmd in `$SolutionDir` and run `docfx Docs/docfx.json`.
1. To preview the docs, run `docfx Docs/docfx.json --serve`.
1. Once the API docs are generated, revert `project.json` and `packages.config` to their old names.
