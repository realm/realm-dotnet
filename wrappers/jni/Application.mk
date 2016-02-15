#NDK_TOOLCHAIN_VERSION := clang
NDK_TOOLCHAIN_VERSION := 4.9
APP_ABI := armeabi armeabi-v7a x86 x86_64 arm64-v8a mips

#  Enable C++14
APP_CPPFLAGS += -std=c++14
APP_CPPFLAGS += -frtti
APP_CPPFLAGS += -fexceptions
APP_CPPFLAGS += -pthread
APP_CPPFLAGS += -DREALM_HAVE_CONFIG=1

# Instruct to use the static GNU STL implementation
APP_STL := gnustl_static

LOCAL_C_INCLUDES += ${ANDROID_NDK}/sources/cxx-stl/gnu-libstdc++/4.9/include
