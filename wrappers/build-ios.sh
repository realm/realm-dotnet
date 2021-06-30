#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REALM_CMAKE_CONFIGURATION=Debug
EXTRA_CMAKE_ARGS="-T buildsystem=1"

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
CMAKE_INSTALL_PREFIX="$SCRIPT_DIRECTORY/build"

mkdir -p "$CMAKE_BINARY_DIR"
mkdir -p "$CMAKE_INSTALL_PREFIX"

function build() {
  cd $CMAKE_BINARY_DIR
  cmake "$SCRIPT_DIRECTORY" -DCMAKE_INSTALL_PREFIX="$CMAKE_INSTALL_PREFIX" -DCMAKE_BUILD_TYPE=$REALM_CMAKE_CONFIGURATION -GXcode $EXTRA_CMAKE_ARGS \
    -DCMAKE_SYSTEM_NAME=iOS \
    -DCMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
    -DCMAKE_XCODE_ATTRIBUTE_DYLIB_INSTALL_NAME_BASE="@rpath" \
    -DCMAKE_TRY_COMPILE_TARGET_TYPE=STATIC_LIBRARY \
    -DCMAKE_TOOLCHAIN_FILE="$SCRIPT_DIRECTORY/realm-core/tools/cmake/ios.toolchain.cmake"

  xcodebuild -scheme realm-wrappers -configuration $REALM_CMAKE_CONFIGURATION -destination "generic/platform=iOS Simulator" -destination generic/platform=iOS

  xcodebuild -create-xcframework \
    -framework "$SCRIPT_DIRECTORY/cmake/iOS/src/$REALM_CMAKE_CONFIGURATION-iphoneos/realm-wrappers.framework" \
    -framework "$SCRIPT_DIRECTORY/cmake/iOS/src/$REALM_CMAKE_CONFIGURATION-iphonesimulator/realm-wrappers.framework" \
    -output "$SCRIPT_DIRECTORY/build/iOS/$REALM_CMAKE_CONFIGURATION/realm-wrappers.xcframework"
}

build