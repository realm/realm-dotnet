# Preprocessing the GA workflows

We're using [ytt](https://github.com/vmware-tanzu/carvel-ytt) to generate the github actions workflows.

## Prerequisites

Get ytt from your package manager of choice. If:
1. on Windows and use chocolatey you can run:  
`choco install ytt -y`
1. on MacOS and use brew you can run:  
`brew tap vmware-tanzu/carvel`  
`brew install ytt`

If instead ytt isn't in your packager manager or you don't use one you can manually get ytt as follows:

1. Download the executable for your platform from the [releases page](https://github.com/vmware-tanzu/carvel-ytt/releases).
1. Rename it to `ytt`.
1. Place it in your PATH.

## Building the docs
1. `cd $SolutionDir/.github/templates`
1. `ytt -f . --output-files ../workflows/` ==> to target all files  
or  
`ytt -f YOUR_FILE --output-files ../workflows/` ==> for a specific file