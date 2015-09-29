#ifndef REALM_CORE_WRAPPER_API

// define the wrapper for linking as a DLL in Windows, otherwise null
#ifdef WIN32
#define REALM_CORE_WRAPPER_API __declspec( dllexport )
#else
#define REALM_CORE_WRAPPER_API
#endif

#endif  // REALM_CORE_WRAPPER_API
