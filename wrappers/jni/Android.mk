LOCAL_PATH := $(call my-dir)

# prepare librealm-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-android
LOCAL_SRC_FILES := ../core-android/$(TARGET_ARCH_ABI)/librealm-android.a
include $(PREBUILT_STATIC_LIBRARY)

#include $(CLEAR_VARS)

OS_SRCS=$(wildcard ../object-store/*.cpp)
ECH_SRCS=../object-store/impl/android/external_commit_helper.cpp
SRCS=$(wildcard ../*.cpp)

LOCAL_MODULE := wrappers
LOCAL_CFLAGS := 
LOCAL_SRC_FILES := $(OS_SRCS)
LOCAL_SRC_FILES += $(ECH_SRCS)
LOCAL_SRC_FILES += $(SRCS)
LOCAL_LDLIBS := -llog
LOCAL_LDLIBS += -lstdc++
LOCAL_CPPFLAGS := -DHAVE_PTHREADS
LOCAL_CPPFLAGS += -DREALM_HAVE_CONFIG
LOCAL_C_INCLUDES += core/include
LOCAL_STATIC_LIBRARIES := realm-android
include $(BUILD_SHARED_LIBRARY)
