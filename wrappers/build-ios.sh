#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REALM_CMAKE_CONFIGURATION=Debug
EXTRA_CMAKE_ARGS=""

for i in "$@"
do
case $i in
  -c=*|--configuration=*)
    REALM_CMAKE_CONFIGURATION="${i#*=}"
    shift
  ;;
  *)
    EXTRA_CMAKE_ARGS="$EXTRA_CMAKE_ARGS $i"
  ;;
esac
done

CMAKE_BINARY_DIR="$SCRIPT_DIRECTORY/cmake/iOS"

mkdir -p "$CMAKE_BINARY_DIR"

function build() {
  cd $CMAKE_BINARY_DIR
  cmake "$SCRIPT_DIRECTORY" -DCMAKE_BUILD_TYPE=$REALM_CMAKE_CONFIGURATION -GXcode $EXTRA_CMAKE_ARGS \
    -DCMAKE_XCODE_ATTRIBUTE_DYLIB_INSTALL_NAME_BASE="@rpath" \
    -DCMAKE_LIBRARY_OUTPUT_DIRECTORY="$SCRIPT_DIRECTORY/build/\$(PLATFORM_NAME)/\$<CONFIG>" \
    -DCMAKE_TOOLCHAIN_FILE="$SCRIPT_DIRECTORY/realm-core/tools/cmake/xcode.toolchain.cmake"

  xcodebuild -scheme realm-wrappers -configuration $REALM_CMAKE_CONFIGURATION -destination "generic/platform=iOS Simulator" -destination generic/platform=iOS
}

build