SDKROOT=$(shell xcrun --sdk iphoneos --show-sdk-path)
#CXX=clang++
CXX=$(shell xcrun --sdk iphoneos --find clang++)
FLAGS=-Icore/include -std=c++11 -stdlib=libc++  

help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean:
	rm -fr build/*
 
libwrappers.dylib:
	$(CXX) $(FLAGS) -DDYNAMIC -Lcore -lc++ -lrealm wrappers.cpp -dynamiclib -arch i386 -o libwrappers.dylib -framework CoreFoundation

 
libwrappers-ios.o:
	$(CXX) $(FLAGS) -c -arch arm64 -arch armv7 wrappers.cpp -o libwrappers-ios.o -isysroot $(SDKROOT)

libwrappers-ios.a: libwrappers-ios.o


	#$(CXX) $(FLAGS) wrappers.cpp -c -lrealm-ios -arch arm64 -arch armv7 -isysroot $(SDKROOT)

.PHONY: all help clean test


