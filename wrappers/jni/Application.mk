#NDK_TOOLCHAIN_VERSION := clang
NDK_TOOLCHAIN_VERSION := 4.8
APP_ABI := armeabi armeabi-v7a x86

#  Enable C++11
APP_CPPFLAGS += -std=c++11
APP_CPPFLAGS += -frtti
APP_CPPFLAGS += -fexceptions
APP_CPPFLAGS += -pthread
APP_CPPFLAGS += -DREALM_HAVE_CONFIG

# Instruct to use the static GNU STL implementation
APP_STL := gnustl_static

LOCAL_C_INCLUDES += ${ANDROID_NDK}/sources/cxx-stl/gnu-libstdc++/4.8/include
