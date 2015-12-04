LOCAL_PATH := $(call my-dir)

# prepare librealm-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-android
LOCAL_SRC_FILES := ../core-android/$(TARGET_ARCH_ABI)/librealm-android.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := wrappers
LOCAL_CFLAGS := 
LOCAL_SRC_FILES := ../object-store/shared_realm.cpp
LOCAL_SRC_FILES += ../object-store/index_set.cpp
LOCAL_SRC_FILES += ../object-store/list.cpp
LOCAL_SRC_FILES += ../object-store/object_schema.cpp
LOCAL_SRC_FILES += ../object-store/object_store.cpp
LOCAL_SRC_FILES += ../object-store/results.cpp
LOCAL_SRC_FILES += ../object-store/schema.cpp
LOCAL_SRC_FILES += ../object-store/impl/transact_log_handler.cpp
LOCAL_SRC_FILES += ../object-store/impl/android/external_commit_helper.cpp

LOCAL_SRC_FILES += ../error_handling.cpp
LOCAL_SRC_FILES += ../linklist_cs.cpp
LOCAL_SRC_FILES += ../marshalling.cpp
LOCAL_SRC_FILES += ../object_schema_cs.cpp
LOCAL_SRC_FILES += ../query_cs.cpp
LOCAL_SRC_FILES += ../realm-csharp.cpp
LOCAL_SRC_FILES += ../row_cs.cpp
LOCAL_SRC_FILES += ../schema_cs.cpp
LOCAL_SRC_FILES += ../shared_realm_cs.cpp
LOCAL_SRC_FILES += ../table_cs.cpp

LOCAL_LDLIBS := -llog
LOCAL_LDLIBS += -lstdc++
LOCAL_CPPFLAGS := -DHAVE_PTHREADS
LOCAL_CPPFLAGS += -DREALM_HAVE_CONFIG=1
LOCAL_C_INCLUDES += core-android/include
LOCAL_C_INCLUDES += object-store/impl/
LOCAL_C_INCLUDES += object-store/impl/android
LOCAL_STATIC_LIBRARIES := realm-android
include $(BUILD_SHARED_LIBRARY)
