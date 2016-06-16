LOCAL_PATH := $(call my-dir)/..

# prepare librealm-android
include $(CLEAR_VARS)

ifdef NDK_DEBUG
LOCAL_MODULE    := librealm-android-dbg
LOCAL_SRC_FILES := core-android/$(TARGET_ARCH_ABI)/librealm-android-dbg.a
else
LOCAL_MODULE    := librealm-android
LOCAL_SRC_FILES := core-android/$(TARGET_ARCH_ABI)/librealm-android.a
endif

include $(PREBUILT_STATIC_LIBRARY)

#include $(CLEAR_VARS)

LOCAL_MODULE := wrappers
LOCAL_CFLAGS := 
LOCAL_SRC_FILES := src/object-store/src/collection_notifications.cpp
LOCAL_SRC_FILES += src/object-store/src/index_set.cpp
LOCAL_SRC_FILES += src/object-store/src/list.cpp
LOCAL_SRC_FILES += src/object-store/src/object_schema.cpp
LOCAL_SRC_FILES += src/object-store/src/object_store.cpp
LOCAL_SRC_FILES += src/object-store/src/results.cpp
LOCAL_SRC_FILES += src/object-store/src/schema.cpp
LOCAL_SRC_FILES += src/object-store/src/shared_realm.cpp
#LOCAL_SRC_FILES += src/object-store/src/parser/parser.cpp
#LOCAL_SRC_FILES += src/object-store/src/parser/query_builder.cpp
LOCAL_SRC_FILES += src/object-store/src/util/format.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/realm_coordinator.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/collection_change_builder.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/collection_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/list_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/results_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/transact_log_handler.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/android/external_commit_helper.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/android/weak_realm_notifier.cpp

LOCAL_SRC_FILES += src/error_handling.cpp
LOCAL_SRC_FILES += src/linklist_cs.cpp
LOCAL_SRC_FILES += src/marshalling.cpp
LOCAL_SRC_FILES += src/query_cs.cpp
LOCAL_SRC_FILES += src/results_cs.cpp
LOCAL_SRC_FILES += src/realm-csharp.cpp
LOCAL_SRC_FILES += src/row_cs.cpp
LOCAL_SRC_FILES += src/schema_cs.cpp
LOCAL_SRC_FILES += src/shared_realm_cs.cpp
LOCAL_SRC_FILES += src/table_cs.cpp
LOCAL_SRC_FILES += src/debug.cpp


LOCAL_LDLIBS := -llog
LOCAL_LDLIBS += -lm
LOCAL_LDLIBS += -landroid
LOCAL_CPPFLAGS := -DHAVE_PTHREADS
LOCAL_CPPFLAGS += -DREALM_HAVE_CONFIG=1
LOCAL_C_INCLUDES += core-android/include
LOCAL_C_INCLUDES += src/object-store/src/

ifdef NDK_DEBUG
LOCAL_STATIC_LIBRARIES := realm-android-dbg
else
LOCAL_STATIC_LIBRARIES := realm-android
endif

include $(BUILD_SHARED_LIBRARY)

