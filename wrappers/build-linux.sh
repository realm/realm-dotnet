#!/bin/bash

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REALM_CMAKE_CONFIGURATION=Debug
EXTRA_CMAKE_ARGS=""
ARCH=x86_64

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

TOOLCHAIN_FILE=""
case $ARCH in
  "x86_64")
    TOOLCHAIN_FILE="x86_64-linux-gnu"
    ;;
  "armhf")
    TOOLCHAIN_FILE="armv7-linux-gnueabihf"
    ;;
  "aarch64")
    TOOLCHAIN_FILE="aarch64-linux-gnu"
    ;;
esac

REALM_CMAKE_SUBPLATFORM="Linux-$libc/$ARCH" bash "$SCRIPT_DIRECTORY"/build.sh -c=$REALM_CMAKE_CONFIGURATION -DCMAKE_TOOLCHAIN_FILE="$SCRIPT_DIRECTORY/realm-core/tools/cmake/$TOOLCHAIN_FILE.toolchain.cmake" -GNinja $EXTRA_CMAKE_ARGS