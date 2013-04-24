// tightdb_c_cs.cpp : Defines the exported functions for the DLL application.
/*
    
Todo:Create unit tests that stresses tightdb with field names that contains all sorts of unicode caracters
that are valid in windows, but do not exist in ansi.  Also create cases with 16bit unicode characters with a zero in them
we should not break easily, and we should know where we have problems.

*/

#include "stdafx.hpp"
#include "tightdb_c_cs.hpp"
#include <iostream>

using namespace tightdb;


#ifdef __cplusplus
extern "C" {
#endif


// return a (manually changed) constant - used when debugging to manually ensure a newly compiled dll is being linked to


 TIGHTDB_C_CS_API size_t tightdb_c_cs_GetVer(void){

  // Table test;
	return 1304241028;
}
	
	

TIGHTDB_C_CS_API size_t table_get_column_count(tightdb::Table* table_ptr)
{
	return table_ptr->get_column_count();
}

TIGHTDB_C_CS_API size_t tableView_get_column_count(tightdb::TableView* tableView_ptr)
{
	return tableView_ptr->get_column_count();
}


//return a newly constructed top level table 
TIGHTDB_C_CS_API Table* new_table()
{
	//return reinterpret_cast<size_t>(LangBindHelper::new_table());	 
	return LangBindHelper::new_table();
}

//todo:tableviews should be invalidated if another tableview or the underlying table is being changed
//this could be implemented by having a connection class, that is local to each process/thread
//and then keep track of the number of views on a given table pointer inside this class
//todo:create unit test that crashes tableview by changing the underlying table

//   TableRef       get_subtable(size_t column_ndx, size_t row_ndx);
TIGHTDB_C_CS_API Table* table_get_subtable(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{    
    return LangBindHelper::get_subtable_ptr(table_ptr,column_ndx, row_ndx);
}

TIGHTDB_C_CS_API void unbind_table_ref(tightdb::Table* table_ptr)
{
	//LangBindHelper::unbind_table_ref(reinterpret_cast<Table*>(table_ptr));
	LangBindHelper::unbind_table_ref(table_ptr);
}


TIGHTDB_C_CS_API size_t table_add_column(tightdb::Table* table_ptr,size_t type, const char* name)
{
    return table_ptr->add_column((DataType)type,name);
}

//    size_t add_column(DataType type, const char* name, ColumnType attr=col_attr_None);
//note that we have omitted support for attr until we figure what it's for
TIGHTDB_C_CS_API size_t spec_add_column(Spec* spec_ptr,size_t type, const char* name) 
{
	return spec_ptr->add_column((DataType)type,name);		
}

//returns the spec that is associated with a table
//this spec is just a handle to use for spec operations and it does not need to be
//unbound or disposed of, it is the address of a spec that is managed by its table
TIGHTDB_C_CS_API Spec* table_get_spec(Table* table_ptr)
{
	//Table* t = reinterpret_cast<Table*>(table_ptr);
	Spec& s = table_ptr->get_spec();
	Spec* spec_ptr  = &s;
	return spec_ptr;
}




//    DataType    get_column_type(size_t column_ndx) const TIGHTDB_NOEXCEPT;
TIGHTDB_C_CS_API  DataType table_get_column_type(Table* table_ptr, const size_t column_ndx)
{
	return table_ptr->get_column_type(column_ndx);
}


TIGHTDB_C_CS_API  tightdb::DataType tableView_get_column_type(tightdb::TableView* tableView_ptr, const size_t column_ndx)
{
	return tableView_ptr->get_column_type(column_ndx);
}






//    DataType    get_column_type(size_t column_ndx) const TIGHTDB_NOEXCEPT;
TIGHTDB_C_CS_API  DataType table_get_mixed_type(Table* table_ptr, const size_t column_ndx,const size_t row_ndx)
{
    return table_ptr->get_mixed_type(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API  void table_update_from_spec(Table* table_ptr)
{
    table_ptr->update_from_spec();
}

TIGHTDB_C_CS_API  DataType spec_get_column_type(Spec* spec_ptr, const size_t column_ndx)
{
	return spec_ptr->get_column_type(column_ndx);
}




//#1 if the string (incl. nulll termination) does not fill in bufsize
//this function returns the desired bufsize needed to hold the string
//also, the buffer might have been updated with data,so the buffer contents is undefined
//in reality as much data as could fit in the buffer has been copied over
//however, the buffer is guarenteed not to have been overrun reg. bufsize
//#2 if the string did fit (including null termination is smaller or eq to bufsize)
//then sbuffer has been filled with the string, and the return value is the size of the string
//null termination excluded
//in general speed is dependent on the size of the src string, not on the size of the buffer
//as it would if strncpy had been used. Also, we do not create a buffer overrun as if strcpy had
//been used, and the string is too small compared to the buffer
//be aware that this code will likely change as soon as tightdb string representation changes
//also note that this function is used for column names as well as column values, so in case tightdb
//strings don't change for column names, but for column string data, two methods are needed, one for  char*
//and one for whatever new stuff we get in tightdb
size_t BSD_strlcpy(char * dst,size_t siz,const char * src)
{
	
	//strlcpy is not supported on many platforms, thus we use the BSD open source version,
	//without the function header, as we need that functionality (safe copy to buffer, size known)
    //the function has been changed to return and take size_t instead of int

	//to comply with copyright, the header below must be included.

/*	$OpenBSD: strlcpy.c,v 1.11 2006/05/05 15:27:38 millert Exp $	*/
/*
 * Copyright (c) 1998 Todd C. Miller <Todd.Miller@courtesan.com>
 *
 * Permission to use, copy, modify, and distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */


/*
 * Copy src to string dst of size siz.  At most siz-1 characters
 * will be copied.  Always NUL terminates (unless siz == 0).
 * Returns strlen(src); if retval >= siz, truncation occurred.
 */
//size_t
//strlcpy(char *dst, const char *src, size_t siz)
//{
	char *d = dst;
	const char *s = src;
	size_t n = siz;

	/* Copy as many bytes as will fit */
	if (n != 0) {
		while (--n != 0) {
			if ((*d++ = *s++) == '\0')
				break;
		}
	}

	/* Not enough room in dst, add NUL and traverse rest of src */
	if (n == 0) {
		if (siz != 0)
			*d = '\0';		/* NUL-terminate dst */
		while (*s++)
			;
	}

	return(s - src - 1);	/* count does not include NUL */
}


//bufsize is the c# capacity that a stringbuilder was created with
//colname is a buffer of at least that size
//the function should copy the column name into colname and zero terminate it, and return the number
//of bytes copied.
//in case the string does not fit inside bufsize, the method simply returns a value that is larger than
//bufsize - this is a request to be passed a buffer of larger size
//however, the buffer might have been filled up with as much data that it could hold acc. to bufsize

//this function will not make buffer overruns even if the column name is longer than the buffer passed
//c# is responsible for memory management of the buffer

TIGHTDB_C_CS_API size_t table_get_column_name(Table* table_ptr,size_t column_ndx,char * colname, size_t bufsize)
{
	const char* cn= table_ptr->get_column_name(column_ndx);
	return BSD_strlcpy(colname,bufsize, cn);
}

TIGHTDB_C_CS_API size_t tableView_get_column_name(TableView* tableView_ptr,size_t column_ndx,char * colname, size_t bufsize)
{
	const char* cn= tableView_ptr->get_column_name(column_ndx);
	return BSD_strlcpy(colname,bufsize, cn);
}

TIGHTDB_C_CS_API size_t spec_get_column_name(Spec* spec_ptr,size_t column_ndx,char * colname, size_t bufsize)
{
	const char* cn= spec_ptr->get_column_name(column_ndx);
	return BSD_strlcpy(colname,bufsize, cn);
}



//    Spec add_subtable_column(const char* name);
TIGHTDB_C_CS_API Spec* spec_add_subtable_column(Spec* spec_ptr, const char* name)
{
	//Spec* s = reinterpret_cast<Spec*>(spec_ptr);
	Spec subtablespec = spec_ptr->add_subtable_column(name);//will add_subtable_column return the address to a spec?
	return new Spec(subtablespec);
	
	//if I understand things correctly, SpecReturn will now BE the spec returned from add_subtable_column Spec IS the address of this class	
	//return reinterpret_cast<size_t>(Ret);//are we returning the address of the spec object returned by add_subtable_column?
}

//deallocate a spec that was allocated in this dll with new
TIGHTDB_C_CS_API void spec_deallocate(Spec* spec_ptr)
{
	delete(spec_ptr);
}

//FIXME: Should we check here on the c++ side, that column_ix is a subtable column before calling
//FIXME: Should this spec be deallocated? or is it part of the table structure it comes from? Currently the C# call is set not to call something similar to unbind_table_ref
TIGHTDB_C_CS_API Spec* spec_get_spec(Spec* spec_ptr,size_t column_ix)
{
    Spec subtablespec = spec_ptr->get_subtable_spec(column_ix);
    return new Spec(subtablespec);//will be unbound later on
}



TIGHTDB_C_CS_API size_t spec_get_column_count(Spec* spec_ptr)
{
    return spec_ptr->get_column_count();
}



TIGHTDB_C_CS_API void table_insert_int(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    table_ptr->insert_int(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void table_set_int(Table*  table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    table_ptr->set_int(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void tableView_set_int(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    tableView_ptr->set_int(column_ndx,row_ndx,value);
}


TIGHTDB_C_CS_API void table_set_mixed_int(Table*  table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    table_ptr->set_mixed(column_ndx,row_ndx,value);
}


TIGHTDB_C_CS_API void tableView_set_mixed_int(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    tableView_ptr->set_mixed(column_ndx,row_ndx,value);
}


TIGHTDB_C_CS_API int64_t  table_get_mixed_int(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_mixed(column_ndx,row_ndx).get_int();    
}

TIGHTDB_C_CS_API int64_t  tableView_get_mixed_int(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableView_ptr->get_mixed(column_ndx,row_ndx).get_int();    
}


TIGHTDB_C_CS_API size_t table_add_empty_row(Table* table_ptr, size_t num_rows)
{
    std::cerr<<"Added a row\n";
    return table_ptr->add_empty_row(num_rows);
}


TIGHTDB_C_CS_API tightdb::TableView* table_find_all_int(Table * table_ptr , size_t column_ndx, int64_t value)
{
     std::cerr<<"fetching tableview \n";
    return new TableView(table_ptr->find_all_int(column_ndx,value));            
 
}

TIGHTDB_C_CS_API void tableview_delete(TableView * tableview_ptr )
{     std::cerr<<"tableview_delete called \n";
    delete(tableview_ptr);
}


TIGHTDB_C_CS_API int64_t table_get_int(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_int(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API int64_t tableView_get_int(TableView* tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableView_ptr->get_int(column_ndx,row_ndx);
}


//only returns false=0  true=1
TIGHTDB_C_CS_API int8_t table_get_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{    
    return table_ptr->get_bool(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API float table_get_float(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_float(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API double table_get_double(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_double(column_ndx,row_ndx);
}


TIGHTDB_C_CS_API size_t table_get_row_count(Table* table_ptr)
{
    return table_ptr->get_column_count();
}



//    static void set_mixed_subtable(Table& parent, std::size_t col_ndx, std::size_t row_ndx,
//                                   const Table& source)

//This method inserts source into the table handled by table_ptr. If source is null, a new table is inserted instead
TIGHTDB_C_CS_API void table_set_mixed_subtable(Table* table_ptr,size_t col_ndx, size_t row_ndx,Table* source)
    {
        LangBindHelper::set_mixed_subtable(*table_ptr,col_ndx,row_ndx,*source);
}

TIGHTDB_C_CS_API void table_set_mixed_empty_subtable(Table* table_ptr,size_t col_ndx, size_t row_ndx)
{     
   table_ptr->set_mixed(col_ndx,row_ndx,Mixed::subtable_tag());//this crashes       
}

TIGHTDB_C_CS_API void tableView_set_mixed_empty_subtable(TableView* tableView_ptr,size_t col_ndx, size_t row_ndx)
{     
   tableView_ptr->set_mixed(col_ndx,row_ndx,Mixed::subtable_tag());//this crashes       
}


TIGHTDB_C_CS_API size_t table_size(Table* table_ptr) 
{
    return table_ptr->size();
}


TIGHTDB_C_CS_API size_t tableview_size(TableView* tableview_ptr) 
{
    return tableview_ptr->size();
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

