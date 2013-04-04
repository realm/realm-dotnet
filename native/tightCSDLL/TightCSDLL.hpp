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

#include<tightdb/spec.hpp>

#ifdef __cplusplus
extern "C" {
#endif


//Table

TIGHTCSDLL_API tightdb::Table* new_table();

TIGHTCSDLL_API void unbind_table_ref(tightdb::Table* TablePtr);

TIGHTCSDLL_API size_t table_get_column_count(tightdb::Table* TablePtr);

//Spec
//type refers to a value taken from DataType values
TIGHTCSDLL_API size_t spec_add_column(tightdb::Spec* SpecPtr,size_t type, const char* name);

TIGHTCSDLL_API tightdb::Spec* spec_add_subtable_column(tightdb::Spec* SpecPtr, const char* name);

TIGHTCSDLL_API tightdb::Spec* table_get_spec(tightdb::Table* TablePtr);

TIGHTCSDLL_API void spec_deallocate(tightdb::Spec* SpecPtr);

TIGHTCSDLL_API int table_get_column_name(tightdb::Table* TablePtr,size_t column_ndx,char * colname, int bufsize);


//non tightdb stuff
TIGHTCSDLL_API size_t fnTightCSDLL(void);

TIGHTCSDLL_API size_t TestIntegerParam(size_t intvalue);

TIGHTCSDLL_API char* TestStringReturn();



#ifdef __cplusplus
}
#endif


