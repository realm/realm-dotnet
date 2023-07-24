#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

bash "$SCRIPT_DIRECTORY"/build.sh -GXcode \
    -DCMAKE_TOOLCHAIN_FILE="${SCRIPT_DIRECTORY}/realm-core/tools/cmake/xcode.toolchain.cmake" \
    -DCMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
    -DCMAKE_SYSTEM_NAME=Darwin \
    -DCMAKE_OSX_ARCHITECTURES="x86_64;arm64" \
    ${@:1}
