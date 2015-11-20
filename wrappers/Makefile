OS_SRCS=$(wildcard object-store/*.cpp)
ECH_SRCS=object-store/impl/apple/external_commit_helper.cpp
SRCS=$(wildcard *.cpp) $(OS_SRCS) $(ECH_SRCS)
OBJS=$(subst .cc,.o,$(SRCS))

SDKROOT=$(shell xcrun --sdk iphoneos --show-sdk-path)
CXX=$(shell xcrun --sdk iphoneos --find clang++)
FLAGS=-Icore/include -Iobject-store -Iobject-store/impl -Iobject-store/impl/apple -std=c++14 -stdlib=libc++  
CORE_VER=0.92.1

help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean: ## Remove everything from build/
	rm -fr build/*
	rm -fr libs/*
 
all: osx ios iosdbg android ## Make all the libs

build:
	mkdir build

.PHONY: all help clean

# OSX and iOS ============

osx: build/Release-osx/libwrappers.dylib 

ios: build/Release-ios-universal/libwrappers.a 

iosdbg: build/Debug-ios-universal/libwrappers.a  

core: realm-core-$(CORE_VER).tar.bz2
	tar xjf realm-core-$(CORE_VER).tar.bz2
	touch core

realm-core-$(CORE_VER).tar.bz2:
	curl -o realm-core-$(CORE_VER).tar.bz2 "http://static.realm.io/downloads/core/realm-core-$(CORE_VER).tar.bz2"

build/Release-osx: | build
	mkdir build/Release-osx

build/Release-osx/libwrappers.dylib: core $(SRCS) | build/Release-osx
	$(CXX) $(FLAGS) -DDYNAMIC -Lcore -lc++ -lrealm $(SRCS) -dynamiclib -arch i386 -o build/Release-osx/libwrappers.dylib -framework CoreFoundation

build/Release-iphoneos/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphoneos -configuration Release -target "wrappers"

build/Release-iphonesimulator/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphonesimulator -configuration Release -target "wrappers"

build/Release-ios-universal: | build
	mkdir build/Release-ios-universal

build/Release-ios-universal/libwrappers.a: build/Release-iphoneos/libwrappers.a build/Release-iphonesimulator/libwrappers.a | build/Release-ios-universal
	lipo -create -output "build/Release-ios-universal/libwrappers.a" "build/Release-iphoneos/libwrappers.a" "build/Release-iphonesimulator/libwrappers.a"

build/Debug-iphoneos/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphoneos -configuration Debug -target "wrappers"

build/Debug-iphonesimulator/libwrappers.a: core $(SRCS)
	xcodebuild -sdk iphonesimulator -configuration Debug -target "wrappers"

build/Debug-ios-universal: | build
	mkdir build/Debug-ios-universal

build/Debug-ios-universal/libwrappers.a: build/Debug-iphoneos/libwrappers.a build/Debug-iphonesimulator/libwrappers.a | build/Debug-ios-universal
	lipo -create -output "build/Debug-ios-universal/libwrappers.a" "build/Debug-iphoneos/libwrappers.a" "build/Debug-iphonesimulator/libwrappers.a"

.PHONY: osx ios iosdbg

# Android ====================

android: build/Release-android/armeabi/libwrappers.so build/Release-android/armeabi-v7a/libwrappers.so build/Release-android/x86/libwrappers.so

build/Release-android: | build
	mkdir build/Release-android

build/Release-android/armeabi: | build/Release-android
	mkdir build/Release-android/armeabi

build/Release-android/armeabi/libwrappers.so: | build/Release-android/armeabi libs
	cp ./libs/armeabi/librealm-android.so ./build/Release-android/armeabi/libwrappers.so

build/Release-android/armeabi-v7a: | build/Release-android
	mkdir build/Release-android/armeabi-v7a

build/Release-android/armeabi-v7a/libwrappers.so: | build/Release-android/armeabi-v7a libs
	cp ./libs/armeabi-v7a/librealm-android.so ./build/Release-android/armeabi-v7a/libwrappers.so

build/Release-android/x86: | build/Release-android
	mkdir build/Release-android/x86

build/Release-android/x86/libwrappers.so: | build/Release-android/x86 libs
	cp ./libs/x86/librealm-android.so ./build/Release-android/x86/libwrappers.so

libs:
	$(NDK_ROOT)/ndk-build APP_PLATFORM=android-9
	

.PHONY: android


