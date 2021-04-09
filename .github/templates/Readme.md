# Preprocessing the GA workflows

We're using [ytt](https://github.com/vmware-tanzu/carvel-ytt) to generate the github actions workflows.

## Prerequisites

1. Download the executable for your platform from the [releases page](https://github.com/vmware-tanzu/carvel-ytt/releases).
1. Rename it to `ytt`.
1. Place it in your PATH.

## Building the docs
1. In `$SolutionDir/.github/templates` run `./process.sh`.
