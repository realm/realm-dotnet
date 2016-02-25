OS_SRCS=$(wildcard src/object-store/src/*.cpp) $(wildcard src/object-store/src/impl/*.cpp)
APPLE_SRCS=$(wildcard src/object-store/src/impl/apple/*.cpp)
ANDROID_SRCS=$(wildcard src/object-store/src/impl/android/*.cpp)
SRCS=$(wildcard src/*.cpp) $(OS_SRCS)

SDKROOT=$(shell xcrun --sdk iphoneos --show-sdk-path)
CXX=$(shell xcrun --sdk iphoneos --find clang++)
FLAGS=-Icore/include -Isrc/object-store/src -Isrc/object-store/src/impl -Isrc/object-store/src/impl/apple -std=c++14 -stdlib=libc++  
CORE_APPLE_VER=0.96.0
CORE_ANDROID_VER=0.96.0

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

build/Release-osx/libwrappers.dylib: core $(SRCS) $(APPLE_SRCS) | build/Release-osx
	$(CXX) $(FLAGS) -DDYNAMIC -Lcore -lc++ -lrealm $(SRCS) $(APPLE_SRCS) -dynamiclib -arch i386 -o build/Release-osx/libwrappers.dylib -framework CoreFoundation

build/Release-iphoneos/libwrappers.a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphoneos -configuration Release -target "wrappers"

build/Release-iphonesimulator/libwrappers.a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphonesimulator -configuration Release -target "wrappers"

build:
	mkdir build

build/Release-ios-universal: | build
	mkdir -p build/Release-ios-universal

build/Release-ios-universal/libwrappers.a: build/Release-iphoneos/libwrappers.a build/Release-iphonesimulator/libwrappers.a | build/Release-ios-universal
	lipo -create -output "build/Release-ios-universal/libwrappers.a" "build/Release-iphoneos/libwrappers.a" "build/Release-iphonesimulator/libwrappers.a"

build/Debug-iphoneos/libwrappers.a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphoneos -configuration Debug -target "wrappers"

build/Debug-iphonesimulator/libwrappers.a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphonesimulator -configuration Debug -target "wrappers"

build/Debug-ios-universal: | build
	mkdir -p build/Debug-ios-universal

build/Debug-ios-universal/libwrappers.a: build/Debug-iphoneos/libwrappers.a build/Debug-iphonesimulator/libwrappers.a | build/Debug-ios-universal
	lipo -create -output "build/Debug-ios-universal/libwrappers.a" "build/Debug-iphoneos/libwrappers.a" "build/Debug-iphonesimulator/libwrappers.a"

.PHONY: osx ios iosdbg

# Android ====================

android: build/Release-android/armeabi/libwrappers.so build/Release-android/armeabi-v7a/libwrappers.so build/Release-android/x86/libwrappers.so build/Release-android/x86_64/libwrappers.so build/Release-android/arm64-v8a/libwrappers.so build/Release-android/mips/libwrappers.so

core-android: realm-core-android-$(CORE_ANDROID_VER).tar.gz
	mkdir -p core-android
	tar xzf realm-core-android-$(CORE_ANDROID_VER).tar.gz -C core-android/
	mkdir -p core-android/armeabi
	mv core-android/librealm-android-arm.a core-android/armeabi/librealm-android.a
	mkdir -p core-android/armeabi-v7a
	mv core-android/librealm-android-arm-v7a.a core-android/armeabi-v7a/librealm-android.a
	mkdir -p core-android/x86
	mv core-android/librealm-android-x86.a core-android/x86/librealm-android.a
	mkdir -p core-android/arm64-v8a
	mv core-android/librealm-android-arm64.a core-android/arm64-v8a/librealm-android.a
	mkdir -p core-android/x86_64
	mv core-android/librealm-android-x86_64.a core-android/x86_64/librealm-android.a
	mkdir -p core-android/mips
	mv core-android/librealm-android-mips.a core-android/mips/librealm-android.a
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

build/Release-android/x86_64:
	mkdir -p build/Release-android/x86_64

build/Release-android/x86_64/libwrappers.so: | build/Release-android/x86_64 libs
	cp ./libs/x86_64/librealm-android.so ./build/Release-android/x86_64/libwrappers.so

build/Release-android/arm64-v8a:
	mkdir -p build/Release-android/arm64-v8a

build/Release-android/arm64-v8a/libwrappers.so: | build/Release-android/arm64-v8a libs
	cp ./libs/arm64-v8a/librealm-android.so ./build/Release-android/arm64-v8a/libwrappers.so

build/Release-android/mips:
	mkdir -p build/Release-android/mips

build/Release-android/mips/libwrappers.so: | build/Release-android/mips libs
	cp ./libs/mips/librealm-android.so ./build/Release-android/mips/libwrappers.so

libs: | core-android $(SRCS) $(ANDROID_SRCS)
	$(NDK_ROOT)/ndk-build APP_PLATFORM=android-10 V=1
	

.PHONY: android


