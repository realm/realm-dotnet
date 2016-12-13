#NDK_TOOLCHAIN_VERSION := clang
NDK_TOOLCHAIN_VERSION := 4.9
APP_ABI := armeabi-v7a x86 x86_64 arm64-v8a

#  Enable C++14
APP_CPPFLAGS += -std=c++14
APP_CPPFLAGS += -frtti
APP_CPPFLAGS += -fexceptions
APP_CPPFLAGS += -pthread
APP_CPPFLAGS += -DREALM_HAVE_CONFIG=1
APP_CPPFLAGS += -DREALM_USE_ALOOPER=0

ifeq ($(REALM_ENABLE_SYNC),1)
APP_CPPFLAGS += -DREALM_ENABLE_SYNC
endif

ifdef NDK_DEBUG
APP_CPPFLAGS += -DREALM_DEBUG=1
endif

# Instruct to use the static GNU STL implementation
APP_STL := gnustl_static

LOCAL_C_INCLUDES += ${ANDROID_NDK}/sources/cxx-stl/gnu-libstdc++/4.9/include
