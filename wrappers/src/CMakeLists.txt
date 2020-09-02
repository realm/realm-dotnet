set(SOURCES
    debug.cpp
    error_handling.cpp
    list_cs.cpp
    marshalling.cpp
    object_cs.cpp
    query_cs.cpp
    realm-csharp.cpp
    results_cs.cpp
    scheduler_cs.cpp
    schema_cs.cpp
    shared_realm_cs.cpp
    table_cs.cpp
)

set(HEADERS
    debug.hpp
    error_handling.hpp
    marshalable_sort_clause.hpp
    marshalling.hpp
    object_cs.hpp
    realm_error_type.hpp
    realm_export_decls.hpp
    schema_cs.hpp
    shared_realm_cs.hpp
)

if(REALM_ENABLE_SYNC)
    list(APPEND SOURCES
        async_open_task_cs.cpp
        sync_manager_cs.cpp
        sync_session_cs.cpp
        sync_user_cs.cpp
        subscription_cs.cpp
    )

    list(APPEND HEADERS
        sync_session_cs.hpp
    )
endif()

if(REALM_ENABLE_SERVER)
    list(APPEND SOURCES
        global_notifier_cs.cpp
    )
endif()

set(INCLUDE_DIRS
    ${REALM_CORE_INCLUDE_DIR}
    ${CMAKE_CURRENT_SOURCE_DIR})

if(${CMAKE_CXX_COMPILER_ID} MATCHES "Clang")
    add_compile_options(-Wno-missing-prototypes)
endif()

add_library(realm-wrappers SHARED ${SOURCES} ${HEADERS})
set_target_properties(realm-wrappers PROPERTIES
  FRAMEWORK ${BUILD_APPLE_FRAMEWORK}
  MACOSX_FRAMEWORK_IDENTIFIER io.realm.wrappers
  MACOSX_FRAMEWORK_SHORT_VERSION_STRING "4.0.0"
  MACOSX_FRAMEWORK_BUNDLE_VERSION "4.0.0"
)
target_compile_definitions(realm-wrappers PRIVATE ${PLATFORM_DEFINES})
target_include_directories(realm-wrappers PUBLIC ${INCLUDE_DIRS})
target_link_libraries(realm-wrappers realm-object-store realm-parser)

if(CMAKE_GENERATOR_PLATFORM)
    set(runtime_target ${CMAKE_SYSTEM_NAME}/$<CONFIG>-${CMAKE_GENERATOR_PLATFORM})
elseif(ANDROID_ABI)
    set(runtime_target ${CMAKE_SYSTEM_NAME}/$<CONFIG>-${ANDROID_ABI})
else()
    set(runtime_target ${CMAKE_SYSTEM_NAME}/$<CONFIG>)
endif()

install(TARGETS realm-wrappers
    RUNTIME DESTINATION ${runtime_target}
    LIBRARY DESTINATION ${runtime_target}
    FRAMEWORK DESTINATION ${runtime_target}
)
if(MSVC)
    install(FILES $<TARGET_PDB_FILE:realm-wrappers> DESTINATION ${runtime_target} OPTIONAL)
endif()