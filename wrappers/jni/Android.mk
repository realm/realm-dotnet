LOCAL_PATH := $(call my-dir)/..

# prepare librealm-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-android
LOCAL_SRC_FILES := core-android/$(TARGET_ARCH_ABI)/librealm-android.a
include $(PREBUILT_STATIC_LIBRARY)

#include $(CLEAR_VARS)

LOCAL_MODULE := wrappers
LOCAL_CFLAGS := 
LOCAL_SRC_FILES := src/object-store/shared_realm.cpp
LOCAL_SRC_FILES += src/object-store/index_set.cpp
LOCAL_SRC_FILES += src/object-store/list.cpp
LOCAL_SRC_FILES += src/object-store/object_schema.cpp
LOCAL_SRC_FILES += src/object-store/object_store.cpp
LOCAL_SRC_FILES += src/object-store/results.cpp
LOCAL_SRC_FILES += src/object-store/schema.cpp
LOCAL_SRC_FILES += src/object-store/impl/transact_log_handler.cpp
LOCAL_SRC_FILES += src/object-store/impl/android/external_commit_helper.cpp

LOCAL_SRC_FILES += src/error_handling.cpp
LOCAL_SRC_FILES += src/linklist_cs.cpp
LOCAL_SRC_FILES += src/marshalling.cpp
LOCAL_SRC_FILES += src/object_schema_cs.cpp
LOCAL_SRC_FILES += src/query_cs.cpp
LOCAL_SRC_FILES += src/realm-csharp.cpp
LOCAL_SRC_FILES += src/row_cs.cpp
LOCAL_SRC_FILES += src/schema_cs.cpp
LOCAL_SRC_FILES += src/shared_realm_cs.cpp
LOCAL_SRC_FILES += src/table_cs.cpp

LOCAL_LDLIBS := -llog
LOCAL_LDLIBS += -lstdc++
LOCAL_CPPFLAGS := -DHAVE_PTHREADS
LOCAL_CPPFLAGS += -DREALM_HAVE_CONFIG=1
LOCAL_C_INCLUDES += core-android/include
LOCAL_C_INCLUDES += src/object-store/impl/
LOCAL_C_INCLUDES += src/object-store/impl/android
LOCAL_STATIC_LIBRARIES := realm-android
include $(BUILD_SHARED_LIBRARY)
