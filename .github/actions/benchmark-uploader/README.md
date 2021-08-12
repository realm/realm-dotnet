# Benchmark uploader

Lorem ipusum

## Basic Usage

A simple workflow script can look like this:

```
name: "Workflow"
on: ["pull_request"]
jobs:
  build:
    runs-on: "ubuntu-latest"
    steps:
      - uses: "actions/checkout@v2"
      - name: "Run benchmarks"
        id: "run-benchmarks"
        uses: "some-benchmark-action"
      - name: "Upload Results"
        uses: ".github/actions/benchmark-uploader"
        with:
          realm-token: ${{ secrets.MDBRealm_Token }}
          file: ${{ steps.run-benchmarks.outputs.benchmark-results }}
          dashboard-path: 'dashboard.charts'
```

### Inputs

| Input | Required | Description |
|-|-|-|
| `realm-token` | `true` | The MongoDB Realm token used to upload the benchmark results. |
| `file` | `true` | The Benchmark.NET results file (in json format). |
| `dashboard-path` | `false` | Optional path to store the MongoDB Charts dashboard generated from the benchmark file. If not provided, no dashboard will be generated. |

## Contributing & Issues

This action is developed as a getting started project and is not extensively tested. If you encounter problems, feel free to open an issue or submit a pull request.
