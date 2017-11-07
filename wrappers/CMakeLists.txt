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
  find_library(REALM_CORE_LIBRARY_LOCATION librealm.a)
  find_library(REALM_CORE_LIBRARY_LOCATION_DEBUG librealm-dbg.a)

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
  download_and_extract_realm_tarball("https://static.realm.io/downloads/sync/realm-sync-cocoa-${REALM_SYNC_VERSION}.tar.xz" ${REALM_SYNC_DIRECTORY} "${REALM_SYNC_LIBRARIES}")
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
elseif(ANDROID AND REALM_ENABLE_SYNC)
  if(ANDROID_ABI STREQUAL "armeabi-v7a")
    set(REALM_SYNC_ANDROID_PLATFORM "android-arm-v7a")
  elseif(ANDROID_ABI STREQUAL "arm64-v8a")
    set(REALM_SYNC_ANDROID_PLATFORM "android-arm64")
  else()
    set(REALM_SYNC_ANDROID_PLATFORM "android-${ANDROID_ABI}")
  endif()
  set(REALM_SYNC_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/realm-sync-${REALM_SYNC_VERSION}")
  set(REALM_CORE_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-android-${ANDROID_ABI}-dbg.a")
  set(REALM_CORE_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-android-${ANDROID_ABI}.a")
  set(REALM_SYNC_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-sync-${REALM_SYNC_ANDROID_PLATFORM}-dbg.a")
  set(REALM_SYNC_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-sync-${REALM_SYNC_ANDROID_PLATFORM}.a")
  set(REALM_SYNC_LIBRARIES ${REALM_CORE_LIBRARY_LOCATION_DEBUG} ${REALM_CORE_LIBRARY_LOCATION} ${REALM_SYNC_LIBRARY_LOCATION_DEBUG} ${REALM_SYNC_LIBRARY_LOCATION})
  download_and_extract_realm_tarball("https://static.realm.io/downloads/sync/realm-sync-android-${REALM_SYNC_VERSION}.tar.gz" ${REALM_SYNC_DIRECTORY} "${REALM_SYNC_LIBRARIES}")
  add_custom_target(extract-realm-tarball DEPENDS ${REALM_SYNC_LIBRARIES})

  add_library(realm STATIC IMPORTED)
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_CORE_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY INTERFACE_LINK_LIBRARIES Threads::Threads ${CRYPTO_LIBRARIES})
  add_dependencies(realm extract-realm-tarball)

  file(MAKE_DIRECTORY ${REALM_SYNC_DIRECTORY}/include)
  set_property(TARGET realm PROPERTY INTERFACE_INCLUDE_DIRECTORIES ${REALM_SYNC_DIRECTORY}/include)

  add_library(realm-sync STATIC IMPORTED)
  set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_SYNC_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm-sync PROPERTY INTERFACE_LINK_LIBRARIES realm ${SSL_LIBRARIES})

  download_android_openssl()
else()
  use_realm_core(${REALM_ENABLE_SYNC} "${REALM_CORE_PREFIX}" "${REALM_SYNC_PREFIX}")
endif()

if(NOT REALM_PLATFORM)
  set(REALM_PLATFORM DotNet) 
endif()

if(APPLE)
  set(CMAKE_OSX_DEPLOYMENT_TARGET 10.10)
elseif(CMAKE_SYSTEM_NAME MATCHES "^Windows")
  add_definitions(-DWIN32_LEAN_AND_MEAN)
  if(WINDOWS_STORE)
    set(CMAKE_SHARED_LINKER_FLAGS_DEBUG "/nodefaultlib:vccorlibd /nodefaultlib:msvcrtd vccorlibd.lib msvcrtd.lib")
    set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "/nodefaultlib:vccorlib /nodefaultlib:msvcrt vccorlib.lib msvcrt.lib")
  else()
    set(CMAKE_SHARED_LINKER_FLAGS_DEBUG "/nodefaultlib:libucrtd.lib /defaultlib:ucrtd.lib")
    set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "/nodefaultlib:libucrt.lib /defaultlib:ucrt.lib")
  endif()
endif()

if(REALM_ENABLE_SYNC)
  add_definitions(-DREALM_ENABLE_SYNC)
endif()

set(PEGTL_INCLUDE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/src/object-store/external/pegtl)

add_subdirectory(src/object-store/src)
add_subdirectory(src)

# force Object Store to use the generic EventLoopSignal implementation
target_compile_definitions(realm-object-store PUBLIC
  REALM_USE_CF=0
  REALM_USE_ALOOPER=0
  REALM_USE_UV=0
)

if(APPLE OR ANDROID)
  # force Object Store to use the generic EventLoopSignal implementation instead of the system one
  # path is relative to object-store/src where add_library(realm-object-store) is called
  target_sources(realm-object-store PRIVATE util/generic/event_loop_signal.cpp)
endif()