// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

//#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#define NOMINMAX  //windows.h defines min and max macros, but these macros conflict with STL http://support.microsoft.com/kb/143208
//#include <windows.h>
#include <limits>
#include <iostream>
#undef NOMINMAX
// TODO: reference additional headers your program requires here
