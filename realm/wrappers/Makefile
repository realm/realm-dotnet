SDKROOT=$(shell xcrun --sdk iphoneos --show-sdk-path)
#CXX=clang++
CXX=$(shell xcrun --sdk iphoneos --find clang++)
FLAGS=-Icore/include -std=c++11 -stdlib=libc++  
CORE_VER=0.92.1

help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean: ## Remove everything from build/
	rm -fr build/*
 
all: build/Release-osx/libwrappers.dylib build/Release-ios-universal/libwrappers.a  ## Make all the libs

core: realm-core-$(CORE_VER).tar.bz2
	tar xjf realm-core-$(CORE_VER).tar.bz2
	touch core

realm-core-$(CORE_VER).tar.bz2:
	curl -o realm-core-$(CORE_VER).tar.bz2 "http://static.realm.io/downloads/core/realm-core-$(CORE_VER).tar.bz2"

build:
	mkdir build

build/Release-osx: | build
	mkdir build/Release-osx

build/Release-osx/libwrappers.dylib: core | build/Release-osx
	$(CXX) $(FLAGS) -DDYNAMIC -Lcore -lc++ -lrealm wrappers.cpp -dynamiclib -arch i386 -o build/Release-osx/libwrappers.dylib -framework CoreFoundation

build/Release-iphoneos/libwrappers.a: core
	xcodebuild -sdk iphoneos -configuration Release -target "wrappers"

build/Release-iphonesimulator/libwrappers.a: core wrappers.cpp wrappers.h
	xcodebuild -sdk iphonesimulator -configuration Release -target "wrappers"

build/Release-ios-universal: | build
	mkdir build/Release-ios-universal

build/Release-ios-universal/libwrappers.a: build/Release-iphoneos/libwrappers.a build/Release-iphonesimulator/libwrappers.a | build/Release-ios-universal
	lipo -create -output "build/Release-ios-universal/libwrappers.a" "build/Release-iphoneos/libwrappers.a" "build/Release-iphonesimulator/libwrappers.a"

build/Release-android/armeabi/libwrappers.so:

	#$(CXX) $(FLAGS) wrappers.cpp -c -lrealm-ios -arch arm64 -arch armv7 -isysroot $(SDKROOT)

.PHONY: all help clean

