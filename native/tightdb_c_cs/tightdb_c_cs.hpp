// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the TIGHTCSDLL_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// TIGHTCSDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef TIGHTDB_C_CS_EXPORTS
#define TIGHTDB_C_CS_API __declspec(dllexport)
#else
#define TIGHTDB_C_CS_API __declspec(dllimport)
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

TIGHTDB_C_CS_API tightdb::Table* new_table();

TIGHTDB_C_CS_API void unbind_table_ref(tightdb::Table* TablePtr);

TIGHTDB_C_CS_API size_t table_get_column_count(tightdb::Table* TablePtr);

TIGHTDB_C_CS_API size_t table_add_column(tightdb::Table* TablePtr,size_t type, const char* name);

//Spec
//type refers to a value taken from DataType values
TIGHTDB_C_CS_API size_t spec_add_column(tightdb::Spec* SpecPtr,size_t type, const char* name);

TIGHTDB_C_CS_API tightdb::Spec* spec_add_subtable_column(tightdb::Spec* SpecPtr, const char* name);

TIGHTDB_C_CS_API tightdb::Spec* table_get_spec(tightdb::Table* TablePtr);

TIGHTDB_C_CS_API void spec_deallocate(tightdb::Spec* SpecPtr);

TIGHTDB_C_CS_API int table_get_column_name(tightdb::Table* TablePtr,size_t column_ndx,char * colname, size_t bufsize);

TIGHTDB_C_CS_API size_t tightdb_c_cs_GetVer(void);



#ifdef __cplusplus
}
#endif


