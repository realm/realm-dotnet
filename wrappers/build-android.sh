#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REALM_CMAKE_CONFIGURATION=Debug
EXTRA_CMAKE_ARGS=""

for i in "$@"
do
case $i in
  -a=*|--ARCH=*)
    ARCH="${i#*=}"
    shift
  ;;
  -c=*|--configuration=*)
    REALM_CMAKE_CONFIGURATION="${i#*=}"
    shift
  ;;
  *)
    EXTRA_CMAKE_ARGS="$EXTRA_CMAKE_ARGS $i"
  ;;
esac
done

function build() {
  REALM_CMAKE_SUBPLATFORM="Android/$1" bash "$SCRIPT_DIRECTORY"/build.sh -DCMAKE_TOOLCHAIN_FILE="${ANDROID_NDK}/build/cmake/android.toolchain.cmake" -DANDROID_ABI=$1 $EXTRA_CMAKE_ARGS
}

export REALM_CMAKE_CONFIGURATION
if [[ "$REALM_CMAKE_CONFIGURATION" = "Release" ]]; then
  export REALM_CMAKE_INSTALL_TARGET="install/strip"
fi

if [[ "$ARCH" ]]; then
  build $ARCH
else
  for arch in armeabi-v7a arm64-v8a x86 x86_64
  do
    build $arch
  done
fi