cmake_minimum_required(VERSION 3.4.0)

set(CMAKE_BUILD_TYPE Debug CACHE STRING "")
project(realm-wrappers)

# Load dependency info from dependencies.list into REALM_FOO_VERSION variables.
set(DEPENDENCIES_FILE "dependencies.list" CACHE STRING "path to dependencies list")
file(STRINGS ${DEPENDENCIES_FILE} DEPENDENCIES)
message("Dependencies: ${DEPENDENCIES}")
foreach(DEPENDENCY IN LISTS DEPENDENCIES)
    string(REGEX MATCHALL "([^=]+)" COMPONENT_AND_VERSION ${DEPENDENCY})
    list(GET COMPONENT_AND_VERSION 0 COMPONENT)
    list(GET COMPONENT_AND_VERSION 1 VERSION)
    set(${COMPONENT} ${VERSION})
endforeach()

function(download_realm_tarball url target libraries)
    get_filename_component(tarball_name "${url}" NAME)

    set(tarball_parent_directory "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}")
    set(tarball_path "${tarball_parent_directory}/${tarball_name}")
    set(temp_tarball_path "/tmp/${tarball_name}")

    if (NOT EXISTS ${tarball_path})
        if (NOT EXISTS ${temp_tarball_path})
            message("Downloading ${url}.")
            file(DOWNLOAD ${url} ${temp_tarball_path}.tmp SHOW_PROGRESS)
            file(RENAME ${temp_tarball_path}.tmp ${temp_tarball_path})
        endif()
        file(COPY ${temp_tarball_path} DESTINATION ${tarball_parent_directory})
    endif()

    if(APPLE)
        add_custom_command(
            COMMENT "Extracting ${tarball_name}"
            OUTPUT ${libraries}
            COMMAND ${CMAKE_COMMAND} -E tar xf ${tarball_path}
            COMMAND ${CMAKE_COMMAND} -E remove_directory ${target}
            COMMAND ${CMAKE_COMMAND} -E rename core ${target}
            COMMAND ${CMAKE_COMMAND} -E touch_nocreate ${libraries})
    elseif(REALM_PLATFORM STREQUAL "Android" OR CMAKE_SYSTEM_NAME MATCHES "^Windows")
        add_custom_command(
            COMMENT "Extracting ${tarball_name}"
            OUTPUT ${libraries}
            COMMAND "${CMAKE_COMMAND}" -E make_directory "${target}"
            COMMAND "${CMAKE_COMMAND}" -E chdir "${target}" "${CMAKE_COMMAND}" -E tar xf "${tarball_path}"
            COMMAND "${CMAKE_COMMAND}" -E touch_nocreate ${libraries})
    endif()
endfunction()

function(download_android_openssl)
    if(ANDROID)
        string(TOLOWER "${CMAKE_BUILD_TYPE}" BUILD_TYPE)
        set(OPENSSL_URL "http://static.realm.io/downloads/openssl/${ANDROID_OPENSSL_VERSION}/Android/${CMAKE_ANDROID_ARCH_ABI}/openssl.tgz")

        message(STATUS "Downloading OpenSSL...s")
        file(DOWNLOAD "${OPENSSL_URL}" "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/openssl.tgz")

        message(STATUS "Uncompressing OpenSSL...")
        file(MAKE_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/openssl")
        execute_process(COMMAND ${CMAKE_COMMAND} -E tar xfz ../openssl.tgz
                        WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/openssl")

        message(STATUS "Importing OpenSSL...")
        set(OpenSSL_DIR "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/openssl/lib/cmake/OpenSSL")
        find_package(OpenSSL REQUIRED CONFIG)
    endif()
endfunction()

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/src/object-store/CMake")

if(NOT REALM_PLATFORM)
  set(REALM_PLATFORM DotNet)
endif()

include(CompilerFlags)

set(REALM_ENABLE_SYNC ON)

if(APPLE)
  find_package(Threads)
  if(CMAKE_SYSTEM_NAME STREQUAL "iOS")
    set(platform ios)
  else()
    set(platform macosx)
  endif()
  set(REALM_SYNC_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/realm-sync-${REALM_SYNC_VERSION}")
  set(REALM_SYNC_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-sync-${platform}-dbg.a")
  set(REALM_SYNC_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-sync-${platform}.a")
  set(REALM_PARSER_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-parser-${platform}-dbg.a")
  set(REALM_PARSER_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-parser-${platform}.a")
  set(REALM_SYNC_LIBRARIES ${REALM_SYNC_LIBRARY_LOCATION_DEBUG} ${REALM_SYNC_LIBRARY_LOCATION} ${REALM_PARSER_LIBRARY_LOCATION_DEBUG} ${REALM_PARSER_LIBRARY_LOCATION})
  download_realm_tarball("https://static.realm.io/downloads/sync/realm-sync-cocoa-${REALM_SYNC_VERSION}.tar.gz" ${REALM_SYNC_DIRECTORY} "${REALM_SYNC_LIBRARIES}")
  add_custom_target(extract-realm-tarball DEPENDS ${REALM_SYNC_LIBRARIES})

  add_library(realm STATIC IMPORTED)
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_SYNC_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY INTERFACE_LINK_LIBRARIES Threads::Threads -lz "-framework Foundation" "-framework Security")
  add_dependencies(realm extract-realm-tarball)

  add_library(realm-parser STATIC IMPORTED)
  set_property(TARGET realm-parser PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_PARSER_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm-parser PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_PARSER_LIBRARY_LOCATION})
  set_property(TARGET realm-parser PROPERTY IMPORTED_LOCATION ${REALM_PARSER_LIBRARY_LOCATION})
  set_property(TARGET realm-parser PROPERTY INTERFACE_LINK_LIBRARIES realm)

  file(MAKE_DIRECTORY ${REALM_SYNC_DIRECTORY}/include)
  set_property(TARGET realm PROPERTY INTERFACE_INCLUDE_DIRECTORIES ${REALM_SYNC_DIRECTORY}/include)

  add_library(realm-sync INTERFACE)
  target_link_libraries(realm-sync INTERFACE realm)
elseif(ANDROID)
  find_package(Threads)
  set(REALM_SYNC_ANDROID_PLATFORM "android-${ANDROID_ABI}")
  set(REALM_SYNC_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/realm-sync-${REALM_SYNC_VERSION}")
  set(REALM_CORE_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-android-${ANDROID_ABI}-dbg.a")
  set(REALM_CORE_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-android-${ANDROID_ABI}.a")
  set(REALM_PARSER_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-parser-android-${ANDROID_ABI}-dbg.a")
  set(REALM_PARSER_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-parser-android-${ANDROID_ABI}.a")
  set(REALM_SYNC_LIBRARY_LOCATION_DEBUG "${REALM_SYNC_DIRECTORY}/librealm-sync-${REALM_SYNC_ANDROID_PLATFORM}-dbg.a")
  set(REALM_SYNC_LIBRARY_LOCATION "${REALM_SYNC_DIRECTORY}/librealm-sync-${REALM_SYNC_ANDROID_PLATFORM}.a")
  set(REALM_SYNC_LIBRARIES ${REALM_CORE_LIBRARY_LOCATION_DEBUG} ${REALM_CORE_LIBRARY_LOCATION} ${REALM_SYNC_LIBRARY_LOCATION_DEBUG} ${REALM_SYNC_LIBRARY_LOCATION})
  download_realm_tarball("https://static.realm.io/downloads/sync/realm-sync-android-${REALM_SYNC_VERSION}.tar.gz" ${REALM_SYNC_DIRECTORY} "${REALM_SYNC_LIBRARIES}")
  add_custom_target(extract-realm-tarball DEPENDS ${REALM_SYNC_LIBRARIES})

  add_library(realm STATIC IMPORTED)
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_CORE_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY IMPORTED_LOCATION ${REALM_CORE_LIBRARY_LOCATION})
  set_property(TARGET realm PROPERTY INTERFACE_LINK_LIBRARIES Threads::Threads OpenSSL::Crypto)
  add_dependencies(realm extract-realm-tarball)

  add_library(realm-parser STATIC IMPORTED)
  set_property(TARGET realm-parser PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_PARSER_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm-parser PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_PARSER_LIBRARY_LOCATION})
  set_property(TARGET realm-parser PROPERTY IMPORTED_LOCATION ${REALM_PARSER_LIBRARY_LOCATION})
  set_property(TARGET realm-parser PROPERTY INTERFACE_LINK_LIBRARIES realm)

  file(MAKE_DIRECTORY ${REALM_SYNC_DIRECTORY}/include)
  set_property(TARGET realm PROPERTY INTERFACE_INCLUDE_DIRECTORIES ${REALM_SYNC_DIRECTORY}/include)

  add_library(realm-sync STATIC IMPORTED)
  set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION_DEBUG ${REALM_SYNC_LIBRARY_LOCATION_DEBUG})
  set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION_RELEASE ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm-sync PROPERTY IMPORTED_LOCATION ${REALM_SYNC_LIBRARY_LOCATION})
  set_property(TARGET realm-sync PROPERTY INTERFACE_LINK_LIBRARIES realm OpenSSL::SSL)

  download_android_openssl()
elseif(WIN32 OR CMAKE_SYSTEM_NAME STREQUAL "Linux")
    set(platform ${CMAKE_SYSTEM_NAME})
    if(CMAKE_GENERATOR_PLATFORM)
        set(platform "${platform}-${CMAKE_GENERATOR_PLATFORM}")
    elseif(ANDROID_ABI)
        set(platform "Android-${ANDROID_ABI}")
    endif()

  set(REALM_CORE_BUILDTREE "${CMAKE_SOURCE_DIR}/../realm-core/CMakeBuild/${CMAKE_SYSTEM_NAME}/${CMAKE_BUILD_TYPE}")
  if(CMAKE_GENERATOR_PLATFORM)
    set(REALM_CORE_BUILDTREE "${REALM_CORE_BUILDTREE}-${CMAKE_GENERATOR_PLATFORM}")
  endif()
  if(EXISTS "${REALM_CORE_BUILDTREE}/realm-config.cmake")
    set(core_config_file "${REALM_CORE_BUILDTREE}/realm-config.cmake")
  else()
    set(core_filename "realm-core-${CMAKE_BUILD_TYPE}-v${REALM_CORE_VERSION}-${platform}-devel")
    if(WINDOWS_STORE)
      string(REPLACE "WindowsStore" "UWP" core_filename ${core_filename})
    endif()

    message(STATUS "Downloading realm-core...")
    file(DOWNLOAD "http://static.realm.io/downloads/core/${core_filename}.tar.gz" "${CMAKE_BINARY_DIR}/${core_filename}.tar.gz")

    message(STATUS "Uncompressing realm-core...")
    file(MAKE_DIRECTORY "${CMAKE_BINARY_DIR}/${core_filename}")
    execute_process(COMMAND ${CMAKE_COMMAND} -E tar xfz "${CMAKE_BINARY_DIR}/${core_filename}.tar.gz"
            WORKING_DIRECTORY "${CMAKE_BINARY_DIR}/${core_filename}")

    if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
      set(core_config_file "${CMAKE_BINARY_DIR}/${core_filename}/lib64/cmake/RealmCore/RealmCoreConfig.cmake")
    else()
      set(core_config_file "${CMAKE_BINARY_DIR}/${core_filename}/lib/cmake/RealmCore/RealmCoreConfig.cmake")
    endif()
  endif()

  include(${core_config_file})

  if(NOT TARGET realm)
    add_library(realm INTERFACE)
    target_link_libraries(realm INTERFACE RealmCore::Storage)
    add_library(realm-parser INTERFACE)
    target_link_libraries(realm-parser INTERFACE RealmCore::QueryParser)
  endif()

  find_package(ZLIB REQUIRED)
  if(WIN32)
    find_package(OpenSSL REQUIRED)
  endif()
  set(REALM_SYNC_BUILDTREE "${CMAKE_SOURCE_DIR}/../realm-sync/CMakeBuild/${CMAKE_SYSTEM_NAME}/${CMAKE_BUILD_TYPE}")
  if(CMAKE_GENERATOR_PLATFORM)
    set(REALM_SYNC_BUILDTREE "${REALM_SYNC_BUILDTREE}-${CMAKE_GENERATOR_PLATFORM}")
  endif()
  if(EXISTS "${REALM_SYNC_BUILDTREE}/realm-sync-config.cmake")
    include("${REALM_SYNC_BUILDTREE}/realm-sync-config.cmake")
  else()
    set(sync_filename "realm-sync-${CMAKE_BUILD_TYPE}-v${REALM_SYNC_VERSION}-${platform}-devel")

    message(STATUS "Downloading realm-sync...")
    file(DOWNLOAD "http://static.realm.io/downloads/sync/${sync_filename}.tar.gz" "${CMAKE_BINARY_DIR}/${sync_filename}.tar.gz")

    message(STATUS "Uncompressing realm-sync...")
    file(MAKE_DIRECTORY "${CMAKE_BINARY_DIR}/${sync_filename}")
    execute_process(COMMAND ${CMAKE_COMMAND} -E tar xfz "${CMAKE_BINARY_DIR}/${sync_filename}.tar.gz"
            WORKING_DIRECTORY "${CMAKE_BINARY_DIR}/${sync_filename}")

    include("${CMAKE_BINARY_DIR}/${sync_filename}/lib/cmake/realm/realm-sync-config.cmake")

    if(NOT TARGET realm-sync)
      add_library(realm-sync INTERFACE)
      target_link_libraries(realm-sync INTERFACE Sync)
    endif()
  endif()
else()
  include(RealmCore)
  use_realm_core(${REALM_ENABLE_SYNC} "${REALM_CORE_PREFIX}" "${REALM_SYNC_PREFIX}")
endif()

set(CMAKE_CXX_VISIBILITY_PRESET hidden)
set(BUILD_APPLE_FRAMEWORK FALSE)

if(APPLE)
  set(CMAKE_CXX_VISIBILITY_PRESET default)
  set(CMAKE_VISIBILITY_INLINES_HIDDEN TRUE)
  if (CMAKE_SYSTEM_NAME STREQUAL "iOS")
    set(CMAKE_OSX_DEPLOYMENT_TARGET 11.0)
    set(BUILD_APPLE_FRAMEWORK TRUE)
  else()
    set(CMAKE_OSX_DEPLOYMENT_TARGET 10.10)
  endif()
elseif(CMAKE_SYSTEM_NAME MATCHES "^Windows")
  add_definitions(-DWIN32_LEAN_AND_MEAN)
  add_definitions(
    /D_DISABLE_EXTENDED_ALIGNED_STORAGE # https://developercommunity.visualstudio.com/comments/279328/view.html
  )

  # always produce a PDB
  set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /Zi")
  set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "${CMAKE_SHARED_LINKER_FLAGS_RELEASE} /DEBUG /OPT:REF /OPT:ICF")

  if(WINDOWS_STORE)
    set(CMAKE_SHARED_LINKER_FLAGS_DEBUG "${CMAKE_SHARED_LINKER_FLAGS_DEBUG} /nodefaultlib:vccorlibd /nodefaultlib:msvcrtd vccorlibd.lib msvcrtd.lib")
    set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "${CMAKE_SHARED_LINKER_FLAGS_RELEASE} /nodefaultlib:vccorlib /nodefaultlib:msvcrt vccorlib.lib msvcrt.lib")
  else()
    set(CMAKE_SHARED_LINKER_FLAGS_DEBUG "${CMAKE_SHARED_LINKER_FLAGS_DEBUG} /nodefaultlib:libucrtd.lib /defaultlib:ucrtd.lib")
    set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "${CMAKE_SHARED_LINKER_FLAGS_RELEASE} /nodefaultlib:libucrt.lib /defaultlib:ucrt.lib")
  endif()
endif()

add_definitions(-DREALM_ENABLE_SYNC)

set(PEGTL_INCLUDE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/src/object-store/external/pegtl)

add_subdirectory(src/object-store/src)
add_subdirectory(src)

# force Object Store to use the generic Scheduler implementation
target_compile_definitions(realm-object-store PUBLIC
  REALM_USE_CF=0
  REALM_USE_ALOOPER=0
  REALM_USE_UV=0
)