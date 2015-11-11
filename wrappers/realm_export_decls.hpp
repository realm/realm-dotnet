/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#ifndef REALM_EXPORT

// define the wrapper for linking as a DLL in Windows, otherwise null
#ifdef WIN32
#define REALM_EXPORT __declspec( dllexport )
#else
#define REALM_EXPORT
#endif

#endif  // REALM_EXPORT
