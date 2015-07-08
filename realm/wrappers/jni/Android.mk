LOCAL_PATH:= $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := wrappers_android
LOCAL_CFLAGS := 
LOCAL_SRC_FILES := ../wrappers.cpp
LOCAL_LDLIBS :=

LOCAL_C_INCLUDES += core/include

APP_STL := gnustl_static

include $(BUILD_SHARED_LIBRARY)
