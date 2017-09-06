# vim:fdm=marker

# Dependencies

include dependencies.list

REALM_ENABLE_SYNC ?= 1

ifeq ($(REALM_ENABLE_SYNC),1)
	APPLE_PREPROCESSOR_DEFINITIONS="REALM_ENABLE_SYNC=true"
	WRAPPERS_XCODE_TARGET=wrappers-sync
	APPLE_BUNDLE_NAME=realm-sync-cocoa-$(REALM_SYNC_VERSION).tar.xz
	APPLE_BUNDLE_URL="http://static.realm.io/downloads/sync/$(APPLE_BUNDLE_NAME)"
else
	APPLE_PREPROCESSOR_DEFINITIONS=""
	WRAPPERS_XCODE_TARGET=wrappers
	APPLE_BUNDLE_NAME=realm-core-cocoa-v$(REALM_CORE_VERSION).tar.xz
	APPLE_BUNDLE_URL="http://static.realm.io/downloads/core/$(APPLE_BUNDLE_NAME)"
endif

OS_SRCS=$(wildcard src/object-store/src/*.cpp) $(wildcard src/object-store/src/impl/*.cpp)
APPLE_SRCS=$(wildcard src/object-store/src/impl/apple/*.cpp)
SRCS=$(wildcard src/*.cpp) $(OS_SRCS)


help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean: ## Remove all build artefacts
	rm -fr build
	rm -fr core
	rm -fr obj
 
all: ios iosdbg

.PHONY: all help clean

# iOS ============ {{{

ios: build/Release-ios-universal/librealm-wrappers.a 

iosdbg: build/Debug-ios-universal/librealm-wrappers.a  

$(APPLE_BUNDLE_NAME):
	bash -c 'export ec=56; while [ $$ec -eq 56 ]; do curl -C - -o $(APPLE_BUNDLE_NAME) $(APPLE_BUNDLE_URL); export ec=$$?; done'

core: $(APPLE_BUNDLE_NAME)
	tar xjf $(APPLE_BUNDLE_NAME)
	touch core

build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphoneos -configuration Release -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"
	xcrun --sdk iphoneos bitcode_strip build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a -r -o build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a

build/Release-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphonesimulator -configuration Release -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"

build/Release-ios-universal:
	mkdir -p build/Release-ios-universal

build/Release-ios-universal/librealm-wrappers.a: build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a build/Release-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a | build/Release-ios-universal
	lipo -create -output "build/Release-ios-universal/librealm-wrappers.a" "build/Release-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a" "build/Release-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a"

build/Debug-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphoneos -configuration Debug -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"

build/Debug-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a: core $(SRCS) $(APPLE_SRCS)
	xcodebuild -sdk iphonesimulator -configuration Debug -target $(WRAPPERS_XCODE_TARGET) EXTRA_PREPROCESSOR_DEFINITIONS="$(APPLE_PREPROCESSOR_DEFINITIONS)"

build/Debug-ios-universal:
	mkdir -p build/Debug-ios-universal

build/Debug-ios-universal/librealm-wrappers.a: build/Debug-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a build/Debug-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a | build/Debug-ios-universal
	lipo -create -output "build/Debug-ios-universal/librealm-wrappers.a" "build/Debug-iphoneos/lib$(WRAPPERS_XCODE_TARGET).a" "build/Debug-iphonesimulator/lib$(WRAPPERS_XCODE_TARGET).a"

.PHONY: osx ios iosdbg

# }}}
