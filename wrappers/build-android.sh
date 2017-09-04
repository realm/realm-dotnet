#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

CONFIGURATION=Debug
EXTRA_CMAKE_ARGS=""

for i in "$@"
do
case $i in
  -a=*|--ARCH=*)
    ARCH="${i#*=}"
    shift
  ;;
  -c=*|--configuration=*)
    CONFIGURATION="${i#*=}"
    shift
  ;;
  *)
    EXTRA_CMAKE_ARGS="$EXTRA_CMAKE_ARGS $i"
  ;;
esac
done

function build() {
  REALM_CMAKE_SUBPLATFORM="Android/$1" bash "$SCRIPT_DIRECTORY"/build.sh -DCMAKE_TOOLCHAIN_FILE="$SCRIPT_DIRECTORY/src/object-store/CMake/android.toolchain.cmake" -DANDROID_ABI=$1 -DREALM_PLATFORM=Android -DCMAKE_BUILD_TYPE=$CONFIGURATION $EXTRA_CMAKE_ARGS
}

if [[ "$ARCH" ]]; then
  build $ARCH
else
  for arch in armeabi-v7a arm64-v8a x86 x86_64
  do
    build $arch
  done
fi