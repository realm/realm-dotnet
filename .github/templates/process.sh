#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

ytt -f main.yml --output-files "${SCRIPT_DIRECTORY}/../workflows"

sed -i '' 's/true:/on:/' "${SCRIPT_DIRECTORY}/../workflows/main.yml"