# Preprocessing the GA workflows

We're using [pkl](http://github.com/apple/pkl) to generate the GitHub actions workflows.

## Prerequisites

Install pkl: https://pkl-lang.org/main/current/pkl-cli/index.html#installation

For macOS, this is simply

```bash
brew install pkl
```

## Building the workflows

Run `pwsh .github/build-workflows.ps1` or the following bash script if you don't have/use powershell:

```bash
cd $SolutionDir/.github/pkl-workflows
pkl eval *.pkl -o ../workflows/%{moduleName}.yml
```
