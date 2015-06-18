//
//  test.cpp
//  silly2
//
//  Created by Kristian Dupont on 16/06/15.
//  Copyright (c) 2015 Kristian Dupont. All rights reserved.
//
#include "stdafx.h"
#include "test.h"

#include <realm/version.hpp>

#ifdef _WIN32 
#define API __declspec(dllexport)
#else
#define API
#endif

#ifdef __cplusplus
extern "C" {
#endif

    API size_t realm_get_wrapper_ver()
    {
        return 20150616;
    }
    
    API int realm_get_ver_minor()
    {
        return realm::Version::get_minor();
    }
    
    
#ifdef __cplusplus
}
#endif

