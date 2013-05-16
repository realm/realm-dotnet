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


//No need to declare header files, as the code is exposed only through the DLL no need to do double work, copying signatures

    /*
TIGHTDB_C_CS_API int64_t tableView_get_int(tightdb::TableView* tableView_ptr, size_t column_ndx, size_t row_ndx);

TIGHTDB_C_CS_API tightdb::Table* new_table();

TIGHTDB_C_CS_API void unbind_table_ref(tightdb::Table* table_ptr);

TIGHTDB_C_CS_API size_t table_get_column_count(tightdb::Table* table_ptr);

TIGHTDB_C_CS_API size_t tableView_get_column_count(tightdb::TableView* table_ptr);

TIGHTDB_C_CS_API size_t table_add_column(tightdb::Table* table_ptr,size_t type, const char* name);

TIGHTDB_C_CS_API size_t table_get_column_name(tightdb::Table* table_ptr,size_t column_ndx,char * colname, size_t bufsize);

//used only when inserting a new row (and then... perhaps it's easier to just use table_set_xxx and start with insert row)
//i guess perhaps using insert will create the row in an atomic operation, while the other operation will get us a row
//that is null, until it gets filled up
TIGHTDB_C_CS_API void table_insert_int(tightdb::Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value);

TIGHTDB_C_CS_API size_t table_add_empty_row(tightdb::Table* table_ptr, size_t num_rows);

TIGHTDB_C_CS_API void table_remove_row(tightdb::Table* table_ptr, size_t row_ndx);

TIGHTDB_C_CS_API tightdb::Spec* table_get_spec(tightdb::Table* table_ptr);

//    int64_t     get_int(size_t column_ndx, size_t row_ndx) const TIGHTDB_NOEXCEPT;

TIGHTDB_C_CS_API int64_t table_get_int(tightdb::Table* table_ptr, size_t column_ndx, size_t row_ndx);


TIGHTDB_C_CS_API float table_get_float(tightdb::Table* table_ptr, size_t column_ndx, size_t row_ndx);

TIGHTDB_C_CS_API double table_get_double(tightdb::Table* table_ptr, size_t column_ndx, size_t row_ndx);

TIGHTDB_C_CS_API size_t table_get_row_count(tightdb::Table*);

TIGHTDB_C_CS_API size_t table_size(tightdb::Table*) ;

//use these to get data from an already saved row
TIGHTDB_C_CS_API void table_set_int(tightdb::Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value);

TIGHTDB_C_CS_API void table_set_mixed_subtable(tightdb::Table* table_ptr,size_t col_ndx, size_t row_ndx,tightdb::Table* source);

TIGHTDB_C_CS_API void table_set_mixed_empty_subtable(tightdb::Table* table_ptr,size_t col_ndx, size_t row_ndx);

TIGHTDB_C_CS_API void table_set_mixed_int(tightdb::Table*  table_ptr, size_t column_ndx, size_t row_ndx, int64_t value);

TIGHTDB_C_CS_API int64_t table_
ed_int(tightdb::Table*  table_ptr, size_t column_ndx, size_t row_ndx);

TIGHTDB_C_CS_API tightdb::Table* table_get_subtable(tightdb::Table* table_ptr, size_t column_ndx, size_t row_ndx);

TIGHTDB_C_CS_API tightdb::TableView* table_find_all_int(tightdb::Table*, size_t column_ndx, int64_t value);
//TIGHTDB_C_CS_API DataType table_get_int(tightdb::Table* table_ptr, size_t column_ndx, size_t row_ndx);

//Spec
//type refers to a value taken from values
TIGHTDB_C_CS_API size_t spec_add_column(tightdb::Spec* spec_ptr,size_t type, const char* name);

TIGHTDB_C_CS_API tightdb::Spec* spec_add_subtable_column(tightdb::Spec* spec_ptr, const char* name);

TIGHTDB_C_CS_API void spec_deallocate(tightdb::Spec* spec_ptr);

TIGHTDB_C_CS_API size_t tightdb_c_cs_GetVer(void);

TIGHTDB_C_CS_API size_t tableview_size(tightdb::TableView* tableview_ptr) ;



*/

#ifdef __cplusplus
}
#endif


