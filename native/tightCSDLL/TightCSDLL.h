// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the TIGHTCSDLL_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// TIGHTCSDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef TIGHTCSDLL_EXPORTS
#define TIGHTCSDLL_API __declspec(dllexport)
#else
#define TIGHTCSDLL_API __declspec(dllimport)
#endif

/*
// This class is not exported from the TightCSDLL.dll
class  CTightCSDLL {
public:
	CTightCSDLL(void);
	// TODO: add your methods here.
};
*/

//  extern TIGHTCSDLL_API int nTightCSDLL;



#ifdef __cplusplus
extern "C" {
#endif
TIGHTCSDLL_API size_t fnTightCSDLL(void);

TIGHTCSDLL_API size_t TestIntegerParam(size_t intvalue);

TIGHTCSDLL_API size_t TestStringReturn();

TIGHTCSDLL_API size_t new_table();

TIGHTCSDLL_API void unbind_table_ref(const size_t TablePtr);

#ifdef __cplusplus
}
#endif

