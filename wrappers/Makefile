OS_SRCS=$(wildcard object-store/*.cpp)
ECH_SRCS=object-store/impl/apple/external_commit_helper.cpp
SRCS=$(wildcard *.cpp) $(OS_SRCS) $(ECH_SRCS)
OBJS=$(subst .cc,.o,$(SRCS))

SDKROOT=$(shell xcrun --sdk iphoneos --show-sdk-path)
CXX=$(shell xcrun --sdk iphoneos --find clang++)
FLAGS=-Icore/include -Iobject-store -Iobject-store/impl -Iobject-store/impl/apple -std=c++14 -stdlib=libc++  
CORE_APPLE_VER=0.95.1
CORE_ANDROID_VER=0.95.1

help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean: ## Remove all build artefacts
	rm -fr build
	rm -fr libs
	rm -fr core
	rm -fr core-android
 
all: ios iosdbg android ## Make all the libs except osx broken - see issue 249

.PHONY: all help clean

# OSX and iOS ============

osx: build/Release-osx/libwrappers.dylib 

ios: build/Release-ios-universal/libwrappers.a 

iosdbg: build/Debug-ios-universal/libwrappers.a  

core: realm-core-$(CORE_APPLE_VER).tar.bz2
	tar xjf realm-core-$(CORE_APPLE_VER).tar.bz2
	touch core

realm-core-$(CORE_APPLE_VER).tar.bz2:
	curl -o realm-core-$(CORE_APPLE_VER).tar.bz2 "http://static.realm.io/downloads/core/realm-core-$(CORE_APPLE_VER).tar.bz2"

build/Release-osx:
	mkdir -p build/Release-osx

build/Release-osx/libwrappers.dylib: core $(SRCS) | build/Release-osx
	$(CXX) $(FLAGS) -DDYNAMIC -Lcore -lc++ -lrealm $(SRCS) -dynamiclib -arch i386 -o build/Release-osx/libwrappers.dylib -framework CoreFoundation

build/Release-iphoneos/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphoneos -configuration Release -target "wrappers"

build/Release-iphonesimulator/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphonesimulator -configuration Release -target "wrappers"

build/Release-ios-universal: | build
	mkdir -p build/Release-ios-universal

build/Release-ios-universal/libwrappers.a: build/Release-iphoneos/libwrappers.a build/Release-iphonesimulator/libwrappers.a | build/Release-ios-universal
	lipo -create -output "build/Release-ios-universal/libwrappers.a" "build/Release-iphoneos/libwrappers.a" "build/Release-iphonesimulator/libwrappers.a"

build/Debug-iphoneos/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphoneos -configuration Debug -target "wrappers-core-debug"

build/Debug-iphonesimulator/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphonesimulator -configuration Debug -target "wrappers-core-debug"

build/Debug-ios-universal: | build
	mkdir -p build/Debug-ios-universal

build/Debug-ios-universal/libwrappers.a: build/Debug-iphoneos/libwrappers.a build/Debug-iphonesimulator/libwrappers.a | build/Debug-ios-universal
	lipo -create -output "build/Debug-ios-universal/libwrappers.a" "build/Debug-iphoneos/libwrappers-core-debug.a" "build/Debug-iphonesimulator/libwrappers-core-debug.a"

.PHONY: osx ios iosdbg

# Android ====================

android: build/Release-android/armeabi/libwrappers.so build/Release-android/armeabi-v7a/libwrappers.so build/Release-android/x86/libwrappers.so

core-android: realm-core-android-$(CORE_ANDROID_VER).tar.gz
	mkdir -p core-android
	tar xzf realm-core-android-$(CORE_ANDROID_VER).tar.gz -C core-android/
	mkdir -p core-android/armeabi
	mv core-android/librealm-android-arm.a core-android/armeabi/librealm-android.a
	mkdir -p core-android/armeabi-v7a
	mv core-android/librealm-android-arm-v7a.a core-android/armeabi-v7a/librealm-android.a
	mkdir -p core-android/x86
	mv core-android/librealm-android-x86.a core-android/x86/librealm-android.a
	touch core-android

realm-core-android-$(CORE_ANDROID_VER).tar.gz:
	curl -o realm-core-android-$(CORE_ANDROID_VER).tar.gz "http://static.realm.io/downloads/core/realm-core-android-$(CORE_ANDROID_VER).tar.gz"

build/Release-android/armeabi:
	mkdir -p build/Release-android/armeabi

build/Release-android/armeabi/libwrappers.so: | build/Release-android/armeabi libs
	cp ./libs/armeabi/librealm-android.so ./build/Release-android/armeabi/libwrappers.so

build/Release-android/armeabi-v7a:
	mkdir -p build/Release-android/armeabi-v7a

build/Release-android/armeabi-v7a/libwrappers.so: | build/Release-android/armeabi-v7a libs
	cp ./libs/armeabi-v7a/librealm-android.so ./build/Release-android/armeabi-v7a/libwrappers.so

build/Release-android/x86:
	mkdir -p build/Release-android/x86

build/Release-android/x86/libwrappers.so: | build/Release-android/x86 libs
	cp ./libs/x86/librealm-android.so ./build/Release-android/x86/libwrappers.so

libs: | core-android
	$(NDK_ROOT)/ndk-build APP_PLATFORM=android-9
	

.PHONY: android


