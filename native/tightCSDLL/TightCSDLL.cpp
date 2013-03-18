// TightCSDLL.cpp : Defines the exported functions for the DLL application.
/*
    


*/

#include "stdafx.h"
#include "TightCSDLL.h"
#include <tightdb.hpp>
//#include <cstdlib>
//#include <string>
//#include "stdint.h" //from tightdb folder
//using tightdb;

// This is an example of an exported variable
#ifdef __cplusplus
extern "C" {
#endif

//TIGHTCSDLL_API int nTightCSDLL=0;

#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C" {
#endif

// This is an example of an exported function.
// used for marshalling testing. This one simply returns the value 423 when called
TIGHTCSDLL_API size_t fnTightCSDLL(void)
{
  // Table test;
	return 423;
}

//test that we can get incoming values
//returns back the double of the value sent
TIGHTCSDLL_API size_t TestIntegerParam(size_t intvalue)
{
	return intvalue * 2;
}

TIGHTCSDLL_API char* TestConstantStringReturn()
{
	char * statictesttext = "Hello from the DLL!";//I assume the text is static and neither on heap or stack or in need of having to be freed
	return statictesttext;
}


#ifdef __cplusplus
}
#endif

/*
// This is the constructor of a class that has been exported.
// see TightCSDLL.h for the class definition
CTightCSDLL::CTightCSDLL()
{
	return;
}
*/

