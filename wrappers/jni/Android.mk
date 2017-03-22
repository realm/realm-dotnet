LOCAL_PATH := $(call my-dir)/..

REALM_ABI := $(TARGET_ARCH_ABI)
ifeq ($(REALM_ABI),armeabi-v7a)
  REALM_ABI := arm-v7a
else ifeq ($(REALM_ABI),arm64-v8a)
  REALM_ABI := arm64
endif

# Prepare librealm-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-android

ifdef NDK_DEBUG
LOCAL_SRC_FILES := core-android/librealm-android-$(REALM_ABI)-dbg.a
else
LOCAL_SRC_FILES := core-android/librealm-android-$(REALM_ABI).a
endif

include $(PREBUILT_STATIC_LIBRARY)

ifeq ($(REALM_ENABLE_SYNC),1)
# Prepare librealm-sync-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-sync-android

ifdef NDK_DEBUG
LOCAL_SRC_FILES := core-android/librealm-sync-android-$(REALM_ABI)-dbg.a
else
LOCAL_SRC_FILES := core-android/librealm-sync-android-$(REALM_ABI).a
endif

include $(PREBUILT_STATIC_LIBRARY)
endif

# And finally make wrappers
include $(CLEAR_VARS)
LOCAL_MODULE := realm-wrappers

LOCAL_CFLAGS := 
LOCAL_SRC_FILES := src/object-store/src/binding_callback_thread_observer.cpp
LOCAL_SRC_FILES += src/object-store/src/collection_notifications.cpp
LOCAL_SRC_FILES += src/object-store/src/index_set.cpp
LOCAL_SRC_FILES += src/object-store/src/list.cpp
LOCAL_SRC_FILES += src/object-store/src/object_schema.cpp
LOCAL_SRC_FILES += src/object-store/src/object_store.cpp
LOCAL_SRC_FILES += src/object-store/src/object.cpp
LOCAL_SRC_FILES += src/object-store/src/results.cpp
LOCAL_SRC_FILES += src/object-store/src/schema.cpp
LOCAL_SRC_FILES += src/object-store/src/shared_realm.cpp
LOCAL_SRC_FILES += src/object-store/src/thread_safe_reference.cpp
LOCAL_SRC_FILES += src/object-store/src/util/format.cpp
LOCAL_SRC_FILES += src/object-store/src/util/generic/event_loop_signal.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/collection_change_builder.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/collection_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/list_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/object_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/realm_coordinator.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/results_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/transact_log_handler.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/weak_realm_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/epoll/external_commit_helper.cpp

LOCAL_SRC_FILES += src/error_handling.cpp
LOCAL_SRC_FILES += src/list_cs.cpp
LOCAL_SRC_FILES += src/marshalling.cpp
LOCAL_SRC_FILES += src/query_cs.cpp
LOCAL_SRC_FILES += src/results_cs.cpp
LOCAL_SRC_FILES += src/realm-csharp.cpp
LOCAL_SRC_FILES += src/shared_realm_cs.cpp
LOCAL_SRC_FILES += src/table_cs.cpp
LOCAL_SRC_FILES += src/schema_cs.cpp
LOCAL_SRC_FILES += src/debug.cpp
LOCAL_SRC_FILES += src/object_cs.cpp
LOCAL_SRC_FILES += src/event_loop_signal_cs.cpp


ifeq ($(REALM_ENABLE_SYNC),1)
LOCAL_SRC_FILES += src/object-store/src/sync/sync_manager.cpp 
LOCAL_SRC_FILES += src/object-store/src/sync/sync_session.cpp
LOCAL_SRC_FILES += src/object-store/src/sync/sync_user.cpp
LOCAL_SRC_FILES += src/object-store/src/sync/impl/sync_file.cpp
LOCAL_SRC_FILES += src/object-store/src/sync/impl/sync_metadata.cpp

LOCAL_SRC_FILES += src/sync_manager_cs.cpp
LOCAL_SRC_FILES += src/sync_user_cs.cpp
LOCAL_SRC_FILES += src/sync_session_cs.cpp

LOCAL_STATIC_LIBRARIES := realm-sync-android
endif

LOCAL_STATIC_LIBRARIES += realm-android

LOCAL_LDLIBS := -llog
LOCAL_LDLIBS += -lm
LOCAL_LDLIBS += -landroid
LOCAL_LDLIBS += -lz
LOCAL_CPPFLAGS := -DHAVE_PTHREADS
LOCAL_C_INCLUDES += core-android/include
LOCAL_C_INCLUDES += src/object-store/src/

include $(BUILD_SHARED_LIBRARY)

