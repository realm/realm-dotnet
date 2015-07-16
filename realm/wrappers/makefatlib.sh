#!/bin/sh
# http://stackoverflow.com/questions/3520977/build-fat-static-library-device-simulator-using-xcode-and-sdk-4

TARGET_NAME="wrappers"

CONFIGURATION=Release
DEVICE=iphoneos
SIMULATOR=iphonesimulator
FAT=universal
OUTPUT=build
LIBRARY_NAME=lib${TARGET_NAME}.a
HEADERS_DIR_NAME=headers

for sdk in ${DEVICE} ${SIMULATOR}
do
  xcodebuild -sdk $sdk -configuration ${CONFIGURATION} -target ${TARGET_NAME} -verbose
done

device_output=${OUTPUT}/${CONFIGURATION}-${DEVICE}
simulator_output=${OUTPUT}/${CONFIGURATION}-${SIMULATOR}
fatlib_output=${OUTPUT}/${CONFIGURATION}-${FAT}

rm -rf "${fatlib_output}"
mkdir -p "${fatlib_output}"
lipo -create -output "${fatlib_output}/${LIBRARY_NAME}" "${device_output}/${LIBRARY_NAME}" "${simulator_output}/${LIBRARY_NAME}"

#headers_dir="${fatlib_output}/${HEADERS_DIR_NAME}"
#mkdir -p "${headers_dir}"
#cp ${TARGET_NAME}/*.h "${headers_dir}"

#open "${fatlib_output}"
