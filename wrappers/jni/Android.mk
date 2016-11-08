LOCAL_PATH := $(call my-dir)/..

#$(error Realm enable sync is $(REALM_ENABLE_SYNC).)

# Prepare librealm-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-android

ifdef NDK_DEBUG
LOCAL_SRC_FILES := core-android/$(TARGET_ARCH_ABI)/librealm-android-dbg.a
else
LOCAL_SRC_FILES := core-android/$(TARGET_ARCH_ABI)/librealm-android.a
endif

include $(PREBUILT_STATIC_LIBRARY)

# Prepare librealm-sync-android
include $(CLEAR_VARS)
LOCAL_MODULE    := librealm-sync-android

ifdef NDK_DEBUG
LOCAL_SRC_FILES := core-android/$(TARGET_ARCH_ABI)/librealm-sync-android-dbg.a
else
LOCAL_SRC_FILES := core-android/$(TARGET_ARCH_ABI)/librealm-sync-android.a
endif

include $(PREBUILT_STATIC_LIBRARY)

# And finally make wrappers
include $(CLEAR_VARS)
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
LOCAL_SRC_FILES += src/object-store/src/thread_confined.cpp
LOCAL_SRC_FILES += src/object-store/src/util/format.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/weak_realm_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/realm_coordinator.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/collection_change_builder.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/collection_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/list_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/results_notifier.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/transact_log_handler.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/handover.cpp
LOCAL_SRC_FILES += src/object-store/src/impl/android/external_commit_helper.cpp

LOCAL_SRC_FILES += src/error_handling.cpp
LOCAL_SRC_FILES += src/linklist_cs.cpp
LOCAL_SRC_FILES += src/marshalling.cpp
LOCAL_SRC_FILES += src/query_cs.cpp
LOCAL_SRC_FILES += src/results_cs.cpp
LOCAL_SRC_FILES += src/realm-csharp.cpp
LOCAL_SRC_FILES += src/shared_realm_cs.cpp
LOCAL_SRC_FILES += src/table_cs.cpp
LOCAL_SRC_FILES += src/schema_cs.cpp
LOCAL_SRC_FILES += src/debug.cpp
LOCAL_SRC_FILES += src/object_cs.cpp

LOCAL_STATIC_LIBRARIES := realm-android

# ifdef REALM_ENABLE_SYNC
#LOCAL_SRC_FILES += src/object-store/src/sync_manager.cpp 
#LOCAL_SRC_FILES += src/object-store/src/sync_metadata.cpp 
#LOCAL_SRC_FILES += src/object-store/src/sync_session.cpp
#LOCAL_SRC_FILES += src/sync_manager_cs.cpp

#LOCAL_STATIC_LIBRARIES += realm-sync-android
# endif

LOCAL_LDLIBS := -llog
LOCAL_LDLIBS += -lm
LOCAL_LDLIBS += -landroid
LOCAL_LDLIBS += -lz
LOCAL_CPPFLAGS := -DHAVE_PTHREADS
LOCAL_CPPFLAGS += -DREALM_HAVE_CONFIG=1
#LOCAL_CPPFLAGS += -DREALM_ENABLE_SYNC
LOCAL_C_INCLUDES += core-android/include
LOCAL_C_INCLUDES += src/object-store/src/

include $(BUILD_SHARED_LIBRARY)

