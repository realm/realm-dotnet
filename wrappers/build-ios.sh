#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REALM_CMAKE_CONFIGURATION=Debug
EXTRA_CMAKE_ARGS=""
export REALM_CMAKE_SUBPLATFORM=iOS

for i in "$@"
do
case $i in
  -c=*|--configuration=*)
    export REALM_CMAKE_CONFIGURATION="${i#*=}"
    shift
  ;;
  *)
    EXTRA_CMAKE_ARGS="$EXTRA_CMAKE_ARGS $i"
  ;;
esac
done

function build() {
  bash "$SCRIPT_DIRECTORY"/build.sh -GXcode $EXTRA_CMAKE_ARGS \
    -DCMAKE_SYSTEM_NAME=iOS \
    -DCMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
    -DCMAKE_XCODE_ATTRIBUTE_ENABLE_BITCODE=YES \
    -DCMAKE_TRY_COMPILE_TARGET_TYPE=STATIC_LIBRARY \
    -DCMAKE_IOS_INSTALL_COMBINED=YES
}

build