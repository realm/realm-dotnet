cmake_minimum_required(VERSION 3.15.0)

set(CMAKE_BUILD_TYPE Debug CACHE STRING "")
project(RealmDotNet)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)

if(ANDROID)
  # On Android LTO builds produce larger binaries, so turn them off
  set(CMAKE_INTERPROCEDURAL_OPTIMIZATION OFF)

  if(CMAKE_INTERPROCEDURAL_OPTIMIZATION)
    STRING(REGEX REPLACE "-O[0-9z]" "-O2" CMAKE_CXX_FLAGS_RELEASE ${CMAKE_CXX_FLAGS_RELEASE})
  else()
    STRING(REGEX REPLACE "-O[0-9z]" "-Oz" CMAKE_CXX_FLAGS_RELEASE ${CMAKE_CXX_FLAGS_RELEASE})
  endif()
endif()

option(REALM_DOTNET_BUILD_CORE_FROM_SOURCE "Build Realm Core from source, as opposed to downloading prebuilt binaries" ON)

if(REALM_DOTNET_BUILD_CORE_FROM_SOURCE)
  set(REALM_BUILD_LIB_ONLY ON)
  set(REALM_ENABLE_SYNC ON)

  add_subdirectory(realm-core EXCLUDE_FROM_ALL)
endif()

# Build as a .framework instead of .dylib
set(BUILD_APPLE_FRAMEWORK OFF)

if(APPLE)
  set(CMAKE_VISIBILITY_INLINES_HIDDEN TRUE)

  if(CMAKE_SYSTEM_NAME STREQUAL "iOS")
    set(BUILD_APPLE_FRAMEWORK TRUE)
  endif()
elseif(CMAKE_SYSTEM_NAME MATCHES "^Windows")
  add_definitions(/DWIN32_LEAN_AND_MEAN)
  add_compile_options(
    /MP # Enable multi-processor compilation
  )

  # always produce a PDB
  set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /Zi")
  set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "${CMAKE_SHARED_LINKER_FLAGS_RELEASE} /DEBUG /OPT:REF /OPT:ICF")

  if(NOT WINDOWS_STORE)
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
  endif()
elseif(CMAKE_HOST_SYSTEM_NAME STREQUAL "Linux")
  if(LINUX_ARCH STREQUAL "arm")
    set(CMAKE_TOOLCHAIN_FILE "${CMAKE_SOURCE_DIR}/vendor/realm-core/tools/cmake/armhf.toolchain.cmake")
    set(CMAKE_IGNORE_PATH "/usr/lib/x86_64-linux-gnu")
  elseif(LINUX_ARCH STREQUAL "arm64")
    set(CMAKE_TOOLCHAIN_FILE "${CMAKE_SOURCE_DIR}/vendor/realm-core/tools/cmake/aarch64.toolchain.cmake")
    set(CMAKE_IGNORE_PATH "/usr/lib/x86_64-linux-gnu")
  endif()
endif()

add_subdirectory(src)
