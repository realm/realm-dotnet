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

REALM_CMAKE_SUBPLATFORM="Linux-$libc/$ARCH" bash "$SCRIPT_DIRECTORY"/build.sh -c=$REALM_CMAKE_CONFIGURATION -DCMAKE_TOOLCHAIN_FILE="$SCRIPT_DIRECTORY/realm-core/tools/cmake/$ARCH.toolchain.cmake" -GNinja $EXTRA_CMAKE_ARGS
