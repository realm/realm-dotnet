#!/bin/sh
$NDK_ROOT/ndk-build APP_PLATFORM=android-9

mv libs build/Release-android
