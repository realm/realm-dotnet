#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REALM_CMAKE_CONFIGURATION=Debug
EXTRA_CMAKE_ARGS=""
ARCH=x64

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

export REALM_CMAKE_CONFIGURATION


if [ $ARCH == "arm" ]; then
  EXTRA_CMAKE_ARGS="$EXTRA_CMAKE_ARGS -DCMAKE_TOOLCHAIN_FILE=$SCRIPT_DIRECTORY/realm-core/tools/cmake/armhf.toolchain.cmake -DCMAKE_IGNORE_PATH=/usr/lib/x86_64-linux-gnu -DZLIB_LIBRARY=/usr/lib/arm-linux-gnueabihf/libz.a -GNinja"
elif [ $ARCH == "arm64" ]; then
  EXTRA_CMAKE_ARGS="$EXTRA_CMAKE_ARGS -DCMAKE_TOOLCHAIN_FILE=$SCRIPT_DIRECTORY/realm-core/tools/cmake/aarch64.toolchain.cmake -DCMAKE_IGNORE_PATH=/usr/lib/x86_64-linux-gnu -DZLIB_LIBRARY=/usr/lib/aarch64-linux-gnu/libz.a -GNinja"
elif [ $ARCH != "x64" ]; then
  echo "Unknown architecture $ARCH"
  exit 1
fi

REALM_CMAKE_SUBPLATFORM="Linux/$ARCH" bash "$SCRIPT_DIRECTORY"/build.sh -c=$REALM_CMAKE_CONFIGURATION $EXTRA_CMAKE_ARGS -DLINUX_ARCH=$ARCH