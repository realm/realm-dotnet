set(SOURCES
	debug.cpp
	error_handling.cpp
	event_loop_signal_cs.cpp
	list_cs.cpp
	marshalling.cpp
	object_cs.cpp
	query_cs.cpp
	realm-csharp.cpp
	results_cs.cpp
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

if(CMAKE_SYSTEM_NAME MATCHES "^Windows")
	list(APPEND SOURCES win32_polyfill.cpp)
endif()

set(INCLUDE_DIRS
    ${REALM_CORE_INCLUDE_DIR}
    ${CMAKE_CURRENT_SOURCE_DIR})

if(CMAKE_SYSTEM_NAME MATCHES "^Windows")
    list(APPEND INCLUDE_DIRS ${REALM_CORE_INCLUDE_DIR}/win32/pthread)
endif()

add_library(realm-wrappers SHARED ${SOURCES} ${HEADERS})
target_compile_definitions(realm-wrappers PRIVATE ${PLATFORM_DEFINES})
target_include_directories(realm-wrappers PUBLIC ${INCLUDE_DIRS})
target_link_libraries(realm-wrappers PUBLIC realm-object-store realm ${PLATFORM_LIBRARIES})

set(runtime_target ${CMAKE_SYSTEM_NAME}/$<CONFIG>-${CMAKE_GENERATOR_PLATFORM})

install(TARGETS realm-wrappers RUNTIME DESTINATION ${runtime_target})
if(MSVC)
	install(FILES $<TARGET_PDB_FILE:realm-wrappers> DESTINATION ${runtime_target} OPTIONAL)
endif()