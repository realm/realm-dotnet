# vim:fdm=marker

# Dependencies

CORE_APPLE_VER=2.1.1
SYNC_APPLE_VER=1.0.0-BETA-3.1

SYNC_ANDROID_VER=1.0.0-BETA-3.1

ANDROID_BUNDLE_NAME=realm-sync-android-$(SYNC_ANDROID_VER).tar.gz
ANDROID_BUNDLE_URL="http://static.realm.io/downloads/sync/$(ANDROID_BUNDLE_NAME)"

ifdef REALM_ENABLE_SYNC
	APPLE_PREPROCESSOR_DEFINITIONS="REALM_ENABLE_SYNC=true"
	WRAPPERS_XCODE_TARGET=wrappers-sync
	APPLE_BUNDLE_NAME=realm-sync-cocoa-$(SYNC_APPLE_VER).tar.xz
	APPLE_BUNDLE_URL="http://static.realm.io/downloads/sync/$(APPLE_BUNDLE_NAME)"
else
	APPLE_PREPROCESSOR_DEFINITIONS=""
	WRAPPERS_XCODE_TARGET=wrappers
	APPLE_BUNDLE_NAME=realm-core-$(CORE_APPLE_VER).tar.xz
	APPLE_BUNDLE_URL="http://static.realm.io/downloads/core/$(APPLE_BUNDLE_NAME)"
endif

OS_SRCS=$(wildcard src/object-store/src/*.cpp) $(wildcard src/object-store/src/impl/*.cpp)
APPLE_SRCS=$(wildcard src/object-store/src/impl/apple/*.cpp)
ANDROID_SRCS=$(wildcard src/object-store/src/impl/android/*.cpp)
SRCS=$(wildcard src/*.cpp) $(OS_SRCS)

SDKROOT=$(shell xcrun --sdk iphoneos --show-sdk-path)
CXX=$(shell xcrun --sdk iphoneos --find clang++)
FLAGS=-Icore/include -Isrc/object-store/src -Isrc/object-store/src/impl -Isrc/object-store/src/impl/apple -std=c++14 -stdlib=libc++  

help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean: ## Remove all build artefacts
	rm -fr build
	rm -fr libs
	rm -fr libs-dbg
	rm -fr core
	rm -fr core-android
 
all: ios iosdbg android androiddbg ## Make all the libs except osx broken - see issue 249

.PHONY: all help clean

# OSX and iOS ============ {{{

osx: build/Release-osx/libwrappers.dylib 

ios: build/Release-ios-universal/libwrappers.a 

iosdbg: build/Debug-ios-universal/libwrappers.a  

$(APPLE_BUNDLE_NAME):
	curl -o $(APPLE_BUNDLE_NAME) $(APPLE_BUNDLE_URL)

core: $(APPLE_BUNDLE_NAME)
	tar xjf $(APPLE_BUNDLE_NAME)
	touch core

build/Release-osx:
	mkdir -p build/Release-osx

build/Release-osx/libwrappers.dylib: core $(SRCS) $(APPLE_SRCS) | build/Release-osx
	$(CXX) $(FLAGS) -DDYNAMIC -Lcore -lc++ -lrealm $(SRCS) $(APPLE_SRCS) -dynamiclib -arch i386 -o build/Release-osx/libwrappers.dylib -framework CoreFoundation

build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphoneos -configuration Release -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"
	xcrun --sdk iphoneos bitcode_strip build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a -r -o build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a

build/Release-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphonesimulator -configuration Release -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"

build:
	mkdir build

build/Release-ios-universal: | build
	mkdir -p build/Release-ios-universal

build/Release-ios-universal/libwrappers.a: build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a build/Release-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a | build/Release-ios-universal
	lipo -create -output "build/Release-ios-universal/libwrappers.a" "build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a" "build/Release-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a"

build/Debug-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphoneos -configuration Debug -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"

build/Debug-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphonesimulator -configuration Debug -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"

build/Debug-ios-universal: | build
	mkdir -p build/Debug-ios-universal

build/Debug-ios-universal/libwrappers.a: build/Debug-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a build/Debug-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a | build/Debug-ios-universal
	lipo -create -output "build/Debug-ios-universal/libwrappers.a" "build/Debug-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a" "build/Debug-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a"

.PHONY: osx ios iosdbg

# }}}

# Android ==================== {{{

android: | libs
	mkdir -p build/Release-android
	cp -r ./libs/ ./build/Release-android

androiddbg: | libs-dbg
	mkdir -p build/Debug-android
	cp -r ./libs-dbg/ ./build/Debug-android

core-android: $(ANDROID_BUNDLE_NAME) | jni/Android.mk jni/Application.mk
	mkdir -p core-android
	tar xzf $(ANDROID_BUNDLE_NAME) -C core-android/
	mkdir -p core-android/armeabi
	mv core-android/librealm-android-arm.a core-android/armeabi/librealm-android.a
	mv core-android/librealm-android-arm-dbg.a core-android/armeabi/librealm-android-dbg.a
	mv core-android/librealm-sync-android-arm.a core-android/armeabi/librealm-sync-android.a
	mv core-android/librealm-sync-android-arm-dbg.a core-android/armeabi/librealm-sync-android-dbg.a
	mkdir -p core-android/armeabi-v7a
	mv core-android/librealm-android-arm-v7a.a core-android/armeabi-v7a/librealm-android.a
	mv core-android/librealm-android-arm-v7a-dbg.a core-android/armeabi-v7a/librealm-android-dbg.a
	mv core-android/librealm-sync-android-arm-v7a.a core-android/armeabi-v7a/librealm-sync-android.a
	mv core-android/librealm-sync-android-arm-v7a-dbg.a core-android/armeabi-v7a/librealm-sync-android-dbg.a
	mkdir -p core-android/x86
	mv core-android/librealm-android-x86.a core-android/x86/librealm-android.a
	mv core-android/librealm-android-x86-dbg.a core-android/x86/librealm-android-dbg.a
	mv core-android/librealm-sync-android-x86.a core-android/x86/librealm-sync-android.a
	mv core-android/librealm-sync-android-x86-dbg.a core-android/x86/librealm-sync-android-dbg.a
	mkdir -p core-android/arm64-v8a
	mv core-android/librealm-android-arm64.a core-android/arm64-v8a/librealm-android.a
	mv core-android/librealm-android-arm64-dbg.a core-android/arm64-v8a/librealm-android-dbg.a
	mv core-android/librealm-sync-android-arm64.a core-android/arm64-v8a/librealm-sync-android.a
	mv core-android/librealm-sync-android-arm64-dbg.a core-android/arm64-v8a/librealm-sync-android-dbg.a
	mkdir -p core-android/x86_64
	mv core-android/librealm-android-x86_64.a core-android/x86_64/librealm-android.a
	mv core-android/librealm-android-x86_64-dbg.a core-android/x86_64/librealm-android-dbg.a
	mv core-android/librealm-sync-android-x86_64.a core-android/x86_64/librealm-sync-android.a
	mv core-android/librealm-sync-android-x86_64-dbg.a core-android/x86_64/librealm-sync-android-dbg.a
	mkdir -p core-android/mips
	mv core-android/librealm-android-mips.a core-android/mips/librealm-android.a
	mv core-android/librealm-android-mips-dbg.a core-android/mips/librealm-android-dbg.a
	mv core-android/librealm-sync-android-mips.a core-android/mips/librealm-sync-android.a
	mv core-android/librealm-sync-android-mips-dbg.a core-android/mips/librealm-sync-android-dbg.a
	touch core-android

$(ANDROID_BUNDLE_NAME):
	curl -o $(ANDROID_BUNDLE_NAME) $(ANDROID_BUNDLE_URL)

libs: | core-android $(SRCS) $(ANDROID_SRCS)
	$(NDK_ROOT)/ndk-build APP_PLATFORM=android-10 V=1 -j$(shell sysctl -n hw.ncpu)
	
libs-dbg: | core-android $(SRCS) $(ANDROID_SRCS)
	$(NDK_ROOT)/ndk-build APP_PLATFORM=android-10 V=1 NDK_DEBUG=1 NDK_LIBS_OUT=libs-dbg -j$(shell sysctl -n hw.ncpu)
	

.PHONY: android androiddbg

# }}}
