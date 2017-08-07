#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

CMAKE_BINARY_DIR="$SCRIPT_DIRECTORY/cmake/$(uname -s)"
CMAKE_INSTALL_PREFIX="$SCRIPT_DIRECTORY/build"

mkdir -p "$CMAKE_BINARY_DIR"
mkdir -p "$CMAKE_INSTALL_PREFIX"

cd $CMAKE_BINARY_DIR
cmake "$SCRIPT_DIRECTORY" -DCMAKE_INSTALL_PREFIX="$CMAKE_INSTALL_PREFIX" ${@:1}
cmake --build . --target install
