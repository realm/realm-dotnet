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
  set(REALM_SYNC_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/realm-sync-${REALM_SYNC_VERSION}")
  set(REALM_SYNC_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-macosx-dbg.a")
  set(REALM_SYNC_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-macosx.a")
  set(REALM_SYNC_LIBRARIES ${REALM_SYNC_LIBRARY_LOCATION_DEBUG} ${REALM_SYNC_LIBRARY_LOCATION})
  download_realm_tarball("https://static.realm.io/downloads/sync/realm-sync-cocoa-${REALM_SYNC_VERSION}.tar.xz" ${REALM_SYNC_DIRECTORY} "${REALM_SYNC_LIBRARIES}")
  add_custom_target(extract-realm-tarball DEPENDS ${REALM_SYNC_LIBRARIES})

  add_library(realm STATIC IMPORTED)
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_SYNC_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY INTERFACE_LINK_LIBRARIES Threads::Threads ${CRYPTO_LIBRARIES} ${SSL_LIBRARIES})
  add_dependencies(realm extract-realm-tarball)

  file(MAKE_DIRECTORY ${REALM_SYNC_DIRECTORY}/include)
  set_property(TARGET realm PROPERTY INTERFACE_INCLUDE_DIRECTORIES ${REALM_SYNC_DIRECTORY}/include)

  add_library(realm-sync INTERFACE)
  target_link_libraries(realm-sync INTERFACE realm)
elseif(CMAKE_SYSTEM_NAME MATCHES "^Windows" AND REALM_CORE_PACKAGING STREQUAL 1)
  set(REALM_CORE_WINDOWS_PLATFORM x86)
  if(CMAKE_GENERATOR_PLATFORM MATCHES "^[Xx]64$")
      set(REALM_CORE_WINDOWS_PLATFORM x64)
  elseif(CMAKE_GENERATOR_PLATFORM MATCHES "^[Aa][Rr][Mm]$")
      set(REALM_CORE_WINDOWS_PLATFORM arm)
  endif()
  if(CMAKE_SYSTEM_NAME STREQUAL "WindowsStore")
    set(REALM_CORE_WINDOWS_TARBALL_SUFFIX -universal)
    set(REALM_CORE_WINDOWS_PLATFORM "uwp-${REALM_CORE_WINDOWS_PLATFORM}")
  else()
    foreach(flag_var
            CMAKE_CXX_FLAGS CMAKE_CXX_FLAGS_DEBUG CMAKE_CXX_FLAGS_RELEASE
            CMAKE_CXX_FLAGS_MINSIZEREL CMAKE_CXX_FLAGS_RELWITHDEBINFO)
      if(${flag_var} MATCHES "/MT")
        string(REGEX REPLACE "/MT" "/MD" ${flag_var} "${${flag_var}}")
      endif()
    endforeach()
  endif()
  
  set(REALM_CORE_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/realm-core-${REALM_CORE_VERSION}")
  set(REALM_CORE_LIBRARY_LOCATION_DEBUG "${REALM_CORE_DIRECTORY}/lib/realm-${REALM_CORE_WINDOWS_PLATFORM}-dbg.lib")
  set(REALM_CORE_LIBRARY_LOCATION "${REALM_CORE_DIRECTORY}/lib/realm-${REALM_CORE_WINDOWS_PLATFORM}.lib")
  set(REALM_CORE_LIBRARIES ${REALM_CORE_LIBRARY_LOCATION_DEBUG} ${REALM_CORE_LIBRARY_LOCATION})
  download_realm_tarball("https://static.realm.io/downloads/core/realm-core-windows${REALM_CORE_WINDOWS_TARBALL_SUFFIX}-${REALM_CORE_VERSION}.zip" ${REALM_CORE_DIRECTORY} "${REALM_CORE_LIBRARIES}")
  add_custom_target(extract-realm-tarball DEPENDS ${REALM_CORE_LIBRARIES})

  add_library(realm STATIC IMPORTED)
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_CORE_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY INTERFACE_LINK_LIBRARIES Threads::Threads ${CRYPTO_LIBRARIES} ${SSL_LIBRARIES})
  add_dependencies(realm extract-realm-tarball)

  file(MAKE_DIRECTORY ${REALM_CORE_DIRECTORY}/include)
  set_property(TARGET realm PROPERTY INTERFACE_INCLUDE_DIRECTORIES ${REALM_CORE_DIRECTORY}/include)
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