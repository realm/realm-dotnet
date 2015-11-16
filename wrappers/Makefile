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
 
all: build/Release-osx/libwrappers.dylib build/Release-ios-universal/libwrappers.a iosdbg ## Make all the libs

iosdbg: build/Debug-ios-universal/libwrappers.a  

core: realm-core-$(CORE_VER).tar.bz2
	tar xjf realm-core-$(CORE_VER).tar.bz2
	touch core

realm-core-$(CORE_VER).tar.bz2:
	curl -o realm-core-$(CORE_VER).tar.bz2 "http://static.realm.io/downloads/core/realm-core-$(CORE_VER).tar.bz2"

build:
	mkdir build

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

build/Release-android/armeabi/libwrappers.so:

.PHONY: all help clean

