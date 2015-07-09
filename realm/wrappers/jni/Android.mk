LOCAL_PATH:= $(call my-dir)

# prepare librealm-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-android
LOCAL_SRC_FILES := $(TARGET_ARCH_ABI)/librealm-android.a
include $(PREBUILT_STATIC_LIBRARY)

#include $(CLEAR_VARS)

LOCAL_MODULE := wrappers_android
LOCAL_CFLAGS := 
LOCAL_SRC_FILES := ../wrappers.cpp
LOCAL_LDLIBS := -llog
LOCAL_LDLIBS += -lstdc++

LOCAL_CPPFLAGS := -DHAVE_PTHREADS

LOCAL_C_INCLUDES += core/include

LOCAL_STATIC_LIBRARIES := realm-android

include $(BUILD_SHARED_LIBRARY)
