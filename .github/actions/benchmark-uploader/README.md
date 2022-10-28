# Benchmark uploader

This is an action that takes the results.json file from a Benchmark.NET run and uploads it to MongoDB Atlas using the realm-web SDK. In the process, it does a couple of things:

1. It enhances the results with git information (i.e. commit, branch, etc.).
2. It generates a charts dashboard that we can then import. This is only actually necessary when the benchmark tests change in any way.
3. It uploads the enhanced results to MongoDB Atlas using Atlas App Services.

## Basic Usage

A simple workflow script can look like this:

```yaml
name: "Workflow"
on: ["pull_request"]
jobs:
  build:
    runs-on: "ubuntu-latest"
    steps:
      - uses: "actions/checkout@v3"
      - name: "Run benchmarks"
        id: "run-benchmarks"
        uses: "some-benchmark-action"
      - name: "Upload Results"
        uses: ".github/actions/benchmark-uploader"
        with:
          realm-token: ${{ secrets.MDBRealm_Token }}
          file: ${{ steps.run-benchmarks.outputs.benchmark-results }}
          nuget-package: ${{ github.workspace }}/Realm/packages/Realm.${{ needs.build-packages.outputs.package_version }}.nupkg
          dashboard-path: 'dashboard.charts'
```

### Inputs

| Input | Required | Description |
|-|-|-|
| `realm-token` | `true` | The Atlas App services token used to upload the benchmark results. |
| `file` | `true` | The Benchmark.NET results file (in json format). |
| `nuget-package` | `true` |  The path to the Realm.nupkg that will be used for file size tracking. |
| `dashboard-path` | `false` | Optional path to store the MongoDB Charts dashboard generated from the benchmark file. If not provided, no dashboard will be generated. |

## MongoDB Services used

We're using the `Realm SDK metrics` project to store benchmark data.

* [MongoDB Atlas](https://cloud.mongodb.com/v2/5f2beb77dd663c59fa806486#clusters) for storing the benchmark results.
* [Atlas App Services](https://realm.mongodb.com/groups/5f2beb77dd663c59fa806486/apps/61153e38bc7c862b9c02c860/dashboard) for auth and uploading new documents.
* [MongoDB Charts](https://charts.mongodb.com/charts-realm-sdk-metrics-yxjvt/dashboards) for plotting the data.
