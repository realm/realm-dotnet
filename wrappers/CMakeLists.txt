cmake_minimum_required(VERSION 3.4.0)

set(CMAKE_BUILD_TYPE Debug CACHE STRING "")
project(realm-wrappers)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/src/object-store/CMake")

if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
  # set before including RealmCore.cmake which calls FindOpenSSL
  set(OPENSSL_USE_STATIC_LIBS TRUE)
endif()

include(CompilerFlags)
include(RealmCore)

set(REALM_ENABLE_SYNC OFF CACHE BOOL "")

if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
  find_library(REALM_CORE_LIBRARY_LOCATION librealm-node.a)
  find_library(REALM_CORE_LIBRARY_LOCATION_DEBUG librealm-node-dbg.a)

  add_library(realm STATIC IMPORTED)
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_CORE_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY INTERFACE_LINK_LIBRARIES Threads::Threads ${CRYPTO_LIBRARIES})

  if(REALM_ENABLE_SYNC)
    find_library(REALM_SYNC_LIBRARY_LOCATION librealm-sync-node.a)
    find_library(REALM_SYNC_LIBRARY_LOCATION_DEBUG librealm-sync-node-dbg.a)

    add_library(realm-sync STATIC IMPORTED)
    set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_SYNC_LIBRARY_LOCATION_DEBUG})
    set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_SYNC_LIBRARY_LOCATION})
    set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION ${REALM_SYNC_LIBRARY_LOCATION})
    set_property(TARGET realm-sync PROPERTY INTERFACE_LINK_LIBRARIES ${SSL_LIBRARIES})
  endif()
elseif(APPLE AND REALM_ENABLE_SYNC)
  download_realm_sync(${REALM_SYNC_VERSION})
  set_property(TARGET realm-sync PROPERTY INTERFACE_LINK_LIBRARIES ${CRYPTO_LIBRARIES} ${SSL_LIBRARIES})
else()
  use_realm_core("${REALM_ENABLE_SYNC}" "${REALM_CORE_PREFIX}" "${REALM_SYNC_PREFIX}")
endif()

if(APPLE)
  set(CMAKE_OSX_DEPLOYMENT_TARGET 10.10)

  # force Object Store to use the generic EventLoopSignal implementation instead of the NSRunLoop-one
  add_definitions(-DREALM_USE_CF=0)
endif()

if(REALM_ENABLE_SYNC)
  add_definitions(-DREALM_ENABLE_SYNC)
endif()

set(PEGTL_INCLUDE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/src/object-store/external/pegtl)

add_subdirectory(src/object-store/src)
add_subdirectory(src)

if(APPLE)
  # force Object Store to use the generic EventLoopSignal implementation instead of the NSRunLoop-one
  # path is relative to object-store/src where add_library(realm-object-store) is called
  target_sources(realm-object-store PRIVATE util/generic/event_loop_signal.cpp)
endif()