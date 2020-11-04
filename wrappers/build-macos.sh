#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

bash "$SCRIPT_DIRECTORY"/build.sh -DCMAKE_TOOLCHAIN_FILE="${SCRIPT_DIRECTORY}/realm-core/tools/cmake/macosx.toolchain.cmake" ${@:1}
