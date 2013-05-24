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


namespace {

//the followng functions convert to/from the types that we know have these features :
//* No marshalling involved - the transfer is fast
//* Blittable to C# types - the transfer is done without changes to the values (fast)
//* Types, that does not change on the c++ side between compileres and platforms
//* Types that have a mirror C# type that behaves the same way on different platforms (like IntPtr and size_t)

//maybe this should be a macro or an inline function. Maybe the compiler inlines it automatically
//bool is stored differently on different c++ compilers so use a size_t instead when p/invoking
inline bool size_t_to_bool(size_t value) 
{
    return value==1;//here i assume 1 and size_t can be compared in a meaningfull way. C# sends a size_t = 1 when true,and =0 when false
}

inline size_t bool_to_size_t(bool value) {
    if(value) return 1;
    return 0;
}


//Date is totally not matched by a C# type, so convert to an int64_t that is interpreted as a 64 bit time_t
//is this memory-safe - will the Date(value) just allocate on stack, so that after the call the stack contains the return value
//until the calle decides to pop it?. If Date(value) becomes a pointer to something on the heap or the like, we have a problem.
inline Date int64_t_to_date(int64_t value){
    return Date(time_t(value));
}

//this call assumes that time_t and int64_t are blittable (same byte size) or that the compiler handles any resizing neccessary
inline int64_t date_to_int64_t(Date value) {
    return value.get_date();
}


//time_t might just in some case have a different size than 64 bits on the c++ side, so let's transfer using int64_t instead
//these two methods are here bc the tightdb i compile against right now does not have had all its time_t parametres changed to Date yet.
inline time_t int64_t_to_time_t(int64_t value) {
    return value;
}

inline int64_t time_t_to_int64_t(time_t value) {
    return value;
}


//as We've got no idea how the compiler represents an instance of DataType on the stack, perhaps it's better to send back a size_t with the value.
//we always know the size of a size_t
inline DataType size_t_to_datatype(size_t value){
    return (DataType)value;//todo:ask if this is a valid typecast. Or would it be better to use e.g int64? or reintepret_cast
}

//as We've got no idea how the compiler represents an instance of DataType on the stack, perhaps it's better to send back a size_t with the value.
//we always know the size of a size_t
inline size_t datatype_to_size_t(DataType value) {
    return (size_t)value;//todo:ask if this is a valid typecast. Or would it be better to use e.g int64? or reintepret_cast
}


} //anonymous namespace

#ifdef __cplusplus
extern "C" {
#endif


// return a (manually changed) constant - used when debugging to manually ensure a newly compiled dll is being linked to


 TIGHTDB_C_CS_API size_t tightdb_c_cs_getver(void){

  // Table test;
	return 1305211510;
}



TIGHTDB_C_CS_API size_t table_get_column_count(tightdb::Table* table_ptr)
{
	return table_ptr->get_column_count();
}

TIGHTDB_C_CS_API size_t tableview_get_column_count(tightdb::TableView* tableView_ptr)
{
	return tableView_ptr->get_column_count();
}


//new empty group
//todo:This fails on windows, awaiting new version (problem is something with the filename, read/write rights where it is put by default)

TIGHTDB_C_CS_API Group* new_group() //should be disposed by calling group_delete
{
    //std::cerr<<"before new group()\n";
    //works Group* g = new Group(Group::unattached_tag());
    //fails  Group* g = new Group();
    Group* g = new Group();
//      std::cerr<<"after new group()\n";
    return g;
//    return new Group();        
}

  TIGHTDB_C_CS_API void test_testacquireanddeletegroup(){

      Group* g  =  new Group("test");     
	delete(g);
}



TIGHTDB_C_CS_API Group* new_group_file(const char* name)//should be disposed by calling group_delete
{
//    std::cerr<<"before new_group_file\n";
//    std::cerr<<"called with name ";
//    std::cerr<<name;
//    std::cerr<<"\n";

    //Group* g = new Group(Group::unattached_tag());
      
      Group* g = new Group(name);
//      std::cerr<<"after new group\n";
    return g;
//    return new Group();        
}

TIGHTDB_C_CS_API Table* group_get_table(Group* group_ptr, char* table_name)//should be disposed by calling unbind_table_ref
{    
    return LangBindHelper::get_table_ptr(group_ptr,table_name);
}


//return a newly constructed top level table 
TIGHTDB_C_CS_API Table* new_table()//should be disposed by calling unbind_table_ref
{
	//return reinterpret_cast<size_t>(LangBindHelper::new_table());	 
	return LangBindHelper::new_table();
}

//todo:tableviews should be invalidated if another tableview or the underlying table is being changed
//this could be implemented by having a connection class, that is local to each process/thread
//and then keep track of the number of views on a given table pointer inside this class
//todo:create unit test that crashes tableview by changing the underlying table

//   TableRef       get_subtable(size_t column_ndx, size_t row_ndx);
TIGHTDB_C_CS_API Table* table_get_subtable(Table* table_ptr, size_t column_ndx, size_t row_ndx)//should be disposed by calling unbind_table_ref
{    
    return LangBindHelper::get_subtable_ptr(table_ptr,column_ndx, row_ndx);
}



TIGHTDB_C_CS_API void unbind_table_ref(tightdb::Table* table_ptr)
{	
	LangBindHelper::unbind_table_ref(table_ptr);
}

TIGHTDB_C_CS_API void table_remove_row(tightdb::Table* table_ptr, size_t row_ndx)
{
    table_ptr->remove(row_ndx);
}

TIGHTDB_C_CS_API void tableview_remove_row(tightdb::TableView* tableView_ptr, size_t row_ndx)
{
    tableView_ptr->remove(row_ndx);
}


TIGHTDB_C_CS_API size_t table_add_column(tightdb::Table* table_ptr,size_t type, const char* name)
{
    return table_ptr->add_column(size_t_to_datatype(type),name);
}

//    size_t add_column(DataType type, const char* name, ColumnType attr=col_attr_None);
//note that we have omitted support for attr until we figure what it's for
TIGHTDB_C_CS_API size_t spec_add_column(Spec* spec_ptr,size_t type, const char* name) 
{
	return spec_ptr->add_column(size_t_to_datatype(type),name);		
}

//returns the spec that is associated with a table
//this spec is just a handle to use for spec operations and it does not need to be
//unbound or disposed of, it is the address of a spec that is managed by its table
TIGHTDB_C_CS_API Spec* table_get_spec(Table* table_ptr)//do not do anything when disposing
{
	//Table* t = reinterpret_cast<Table*>(table_ptr);
	Spec& s = table_ptr->get_spec();
	Spec* spec_ptr  = &s;
	return spec_ptr;
}




//    DataType    get_column_type(size_t column_ndx) const TIGHTDB_NOEXCEPT;
TIGHTDB_C_CS_API  size_t table_get_column_type(Table* table_ptr, const size_t column_ndx)
{
	return datatype_to_size_t(table_ptr->get_column_type(column_ndx));
}


TIGHTDB_C_CS_API  size_t tableview_get_column_type(tightdb::TableView* tableView_ptr, const size_t column_ndx)
{
	return datatype_to_size_t(tableView_ptr->get_column_type(column_ndx));
}




TIGHTDB_C_CS_API  size_t tableview_get_mixed_type(TableView* tableView_ptr, const size_t column_ndx,const size_t row_ndx)
{
    return datatype_to_size_t(tableView_ptr->get_mixed_type(column_ndx,row_ndx));
}



//    DataType    get_column_type(size_t column_ndx) const TIGHTDB_NOEXCEPT;
TIGHTDB_C_CS_API  size_t table_get_mixed_type(Table* table_ptr, const size_t column_ndx,const size_t row_ndx)
{
    return datatype_to_size_t(table_ptr->get_mixed_type(column_ndx,row_ndx));

    //this here below is debug stuff
}

TIGHTDB_C_CS_API  void table_update_from_spec(Table* table_ptr)
{
    table_ptr->update_from_spec();
}

TIGHTDB_C_CS_API  size_t spec_get_column_type(Spec* spec_ptr, const size_t column_ndx)
{
   
    return datatype_to_size_t(spec_ptr->get_column_type(column_ndx));
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
size_t bsd_strlcpy(char * dst,size_t siz,const char * src)
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
	return bsd_strlcpy(colname,bufsize, cn);
}

//todo:Csharp will currently treat the returned data as ansi with the current codepage - this will be fixed at the same time as the new tightdb strings are released,
//as the conversion from utc-8 to utc-16 will be done on the c++ side
TIGHTDB_C_CS_API size_t table_get_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, char * datatochsarp, size_t bufsize)
{
    const char* fielddata=table_ptr->get_string(column_ndx, row_ndx);
    return bsd_strlcpy(datatochsarp,bufsize,fielddata);
}

TIGHTDB_C_CS_API size_t tableview_get_string(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx, char * datatochsarp, size_t bufsize)
{
    const char* fielddata=tableview_ptr->get_string(column_ndx, row_ndx);
    return bsd_strlcpy(datatochsarp,bufsize,fielddata);
}

TIGHTDB_C_CS_API size_t tableview_get_column_name(TableView* tableView_ptr,size_t column_ndx,char * colname, size_t bufsize)
{
	const char* cn= tableView_ptr->get_column_name(column_ndx);
	return bsd_strlcpy(colname,bufsize, cn);
}

TIGHTDB_C_CS_API size_t spec_get_column_name(Spec* spec_ptr,size_t column_ndx,char * colname, size_t bufsize)
{
	const char* cn= spec_ptr->get_column_name(column_ndx);
	return bsd_strlcpy(colname,bufsize, cn);
}



//    Spec add_subtable_column(const char* name);
TIGHTDB_C_CS_API Spec* spec_add_subtable_column(Spec* spec_ptr, const char* name)//the returned spec should be disposed of by calling spec_deallocate
{	
	Spec subtablespec = spec_ptr->add_subtable_column(name);//will add_subtable_column return the address to a spec?
	return new Spec(subtablespec);
}



TIGHTDB_C_CS_API void table_set_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, const char* value)
{
    table_ptr->set_string(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void tableview_set_string(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx, const char* value)
{
    tableview_ptr->set_string(column_ndx,row_ndx,value);
}


//deallocate a spec that was allocated in this dll with new
TIGHTDB_C_CS_API void spec_deallocate(Spec* spec_ptr)
{
	delete(spec_ptr);
}

TIGHTDB_C_CS_API Spec* spec_get_spec(Spec* spec_ptr,size_t column_ix)//should be disposed by calling spec_deallocate
{
    Spec subtablespec = spec_ptr->get_subtable_spec(column_ix);
    return new Spec(subtablespec);
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

TIGHTDB_C_CS_API void tableview_set_int(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    tableView_ptr->set_int(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void table_set_double(Table*  table_ptr, size_t column_ndx, size_t row_ndx, double value)
{
    table_ptr->set_double(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void tableview_set_double(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, double value)
{
    tableView_ptr->set_double(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void table_set_float(Table*  table_ptr, size_t column_ndx, size_t row_ndx, float value)
{
    table_ptr->set_float(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void tableview_set_float(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, float value)
{
    tableView_ptr->set_float(column_ndx,row_ndx,value);
}



//assuming that int64_t and time_t are binary compatible and of equal size
//the int64_t has been set to a valid time_t date by C#
TIGHTDB_C_CS_API void tableview_set_date(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    tableView_ptr->set_date(column_ndx,row_ndx,int64_t_to_time_t(value));
}

//assuming that int64_t and time_t are binary compatible and of equal size
TIGHTDB_C_CS_API void table_set_date(Table*  table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    table_ptr->set_date(column_ndx,row_ndx,int64_t_to_time_t(value));
}


TIGHTDB_C_CS_API void table_set_mixed_int(Table*  table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    table_ptr->set_mixed(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void tableview_set_mixed_float(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, float value)
{
    tableView_ptr->set_mixed(column_ndx,row_ndx,value);
}


TIGHTDB_C_CS_API void table_set_mixed_float(Table*  table_ptr, size_t column_ndx, size_t row_ndx, float value)
{
    table_ptr->set_mixed(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void table_set_mixed_double(Table*  table_ptr, size_t column_ndx, size_t row_ndx, double value)
{
    table_ptr->set_mixed(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void tableview_set_mixed_double(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, double value)
{
    tableView_ptr->set_mixed(column_ndx,row_ndx,value);
}

TIGHTDB_C_CS_API void tableview_set_mixed_int(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    tableView_ptr->set_mixed(column_ndx,row_ndx,value);
}

//todo:careful unit test that ensures that the stack size matches up on 64 and 32 bit
//the time_t will be marshalled from C# as a 64 bit integer. On the few platforms that I know of, time_t does NOT follow size_t when 32bit.
//if we stumble upon a platform that stores time_t in a 32 bit value, we might have to have a function that returns the size of time_t
TIGHTDB_C_CS_API void tableview_set_mixed_date(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    tableView_ptr->set_mixed(column_ndx,row_ndx,int64_t_to_date(value));
}

//todo:careful unit test that ensures that the stack size matches up on 64 and 32 bit
//the time_t will be marshalled from C# as a 64 bit integer. On the few platforms that I know of, time_t does NOT follow size_t when 32bit.
//if we stumble upon a platform that stores time_t in a 32 bit value, we might have to have a function that returns the size of time_t
TIGHTDB_C_CS_API void table_set_mixed_date(Table*  table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
/*    
       std::cerr<<"table_set_mixed_date got("<<value <<")\n";
    table_ptr->set_mixed(column_ndx,row_ndx,int64_t_to_Date(value));

    Mixed m = table_ptr->get_mixed(column_ndx,row_ndx);
    
    DataType t = m.get_type();

    time_t retval = m.get_date();    

     std::cerr<<"datatype ("<<t<<")";
     std::cerr<<"retval ("<<retval<<")";

     int64_t retval2 = time_t_to_int64_t(retval);

       std::cerr<<"table_get_mixed_date returns("<<retval2<<")\n";
       */
//original code!
    table_ptr->set_mixed(column_ndx,row_ndx,int64_t_to_date(value));

}

TIGHTDB_C_CS_API int64_t tableview_get_mixed_date(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    /*
    int64_t retval = time_t_to_int64_t(tableView_ptr->get_mixed(column_ndx,row_ndx).get_date());

       std::cerr<<"table_view_get_mixed_date returns("<<retval<<")\n";
    return retval;
    */
    return time_t_to_int64_t(tableView_ptr->get_mixed(column_ndx,row_ndx).get_date());
}

TIGHTDB_C_CS_API int64_t table_get_mixed_date(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
    /*
 int64_t retval = time_t_to_int64_t(table_ptr->get_mixed(column_ndx,row_ndx).get_date());    
       std::cerr<<"table_get_mixed_date returns("<<retval<<")\n";
    return retval;
 */
   return time_t_to_int64_t(table_ptr->get_mixed(column_ndx,row_ndx).get_date());    
}

TIGHTDB_C_CS_API int64_t tableview_get_date(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return time_t_to_int64_t(tableView_ptr->get_date(column_ndx,row_ndx));
}

TIGHTDB_C_CS_API int64_t table_get_date(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
    return time_t_to_int64_t(table_ptr->get_date(column_ndx,row_ndx));
}

TIGHTDB_C_CS_API int64_t  table_get_mixed_int(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_mixed(column_ndx,row_ndx).get_int();    
}

TIGHTDB_C_CS_API double  table_get_mixed_double(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_mixed(column_ndx,row_ndx).get_double();    
}

TIGHTDB_C_CS_API float  table_get_mixed_float(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_mixed(column_ndx,row_ndx).get_float();    
}


TIGHTDB_C_CS_API int64_t  tableview_get_mixed_int(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableView_ptr->get_mixed(column_ndx,row_ndx).get_int();    
}

TIGHTDB_C_CS_API double  tableview_get_mixed_double(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableView_ptr->get_mixed(column_ndx,row_ndx).get_double();    
}

TIGHTDB_C_CS_API float  tableview_get_mixed_float(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableView_ptr->get_mixed(column_ndx,row_ndx).get_float();    
}


TIGHTDB_C_CS_API size_t table_add_empty_row(Table* table_ptr, size_t num_rows)
{
    
    return table_ptr->add_empty_row(num_rows);
}

//find first methods in Table
/* 
    size_t         find_first_int(size_t column_ndx, int64_t value) const;
    size_t         find_first_bool(size_t column_ndx, bool value) const;
    size_t         find_first_date(size_t column_ndx, time_t value) const;
    size_t         find_first_float(size_t column_ndx, float value) const;
    size_t         find_first_double(size_t column_ndx, double value) const;
    size_t         find_first_string(size_t column_ndx, const char* value) const;
    size_t         find_first_binary(size_t column_ndx, const char* value, size_t len) const;
    */

TIGHTDB_C_CS_API size_t table_find_first_binary(Table * table_ptr , size_t column_ndx, char* value, size_t len)
{   
    return  table_ptr->find_first_binary(column_ndx,value,len);
}


TIGHTDB_C_CS_API size_t table_find_first_string(Table * table_ptr , size_t column_ndx, char* value)
{   
    return  table_ptr->find_first_string(column_ndx,value);
}



TIGHTDB_C_CS_API size_t table_find_first_double(Table * table_ptr , size_t column_ndx, double value)
{   
    return  table_ptr->find_first_double(column_ndx,value);
}


TIGHTDB_C_CS_API size_t table_find_first_float(Table * table_ptr , size_t column_ndx, float value)
{   
    return  table_ptr->find_first_float(column_ndx,value);
}

//assuming int64_t and time_t are binary compatible.
TIGHTDB_C_CS_API size_t table_find_first_date(Table * table_ptr , size_t column_ndx, int64_t value)
{   
    return  table_ptr->find_first_date(column_ndx,int64_t_to_time_t(value));
}

TIGHTDB_C_CS_API size_t table_find_first_bool(Table * table_ptr , size_t column_ndx, size_t value)
{   
    return  table_ptr->find_first_bool(column_ndx,size_t_to_bool(value));
}

//size_t         find_first_bool(size_t column_ndx, bool value) const;
TIGHTDB_C_CS_API size_t table_find_first_int(Table * table_ptr , size_t column_ndx, int64_t value)
{   
    return table_ptr->find_first_int(column_ndx,value);
}


/*
find first methods in tableview
    size_t find_first_int(size_t column_ndx, int64_t value) const;
    size_t find_first_bool(size_t column_ndx, bool value) const;
    size_t find_first_date(size_t column_ndx, time_t value) const;
    size_t find_first_float(size_t column_ndx, float value) const;
    size_t find_first_double(size_t column_ndx, double value) const;
    size_t find_first_string(size_t column_ndx, const char* value) const;
    find_first_binary is not implemented in tableview yet
*/

/*
TIGHTDB_C_CS_API size_t tableView_find_first_binary(TableView * table_ptr , size_t column_ndx, char* value, size_t len)
{   
    return  table_ptr->find_first_binary(column_ndx,value,len);
}
*/

TIGHTDB_C_CS_API size_t tableview_find_first_string(TableView * table_ptr , size_t column_ndx, char* value)
{   
    return  table_ptr->find_first_string(column_ndx,value);
}



TIGHTDB_C_CS_API size_t tableview_find_first_double(TableView * table_ptr , size_t column_ndx, double value)
{   
    return  table_ptr->find_first_double(column_ndx,value);
}


TIGHTDB_C_CS_API size_t tableview_find_first_float(TableView * table_ptr , size_t column_ndx, float value)
{   
    return  table_ptr->find_first_float(column_ndx,value);
}

//assuming int64_t and time_t are binary compatible.
TIGHTDB_C_CS_API size_t tableview_find_first_date(TableView * table_ptr , size_t column_ndx, int64_t value)
{   
    return  table_ptr->find_first_date(column_ndx,value);
}

TIGHTDB_C_CS_API size_t tableview_find_first_bool(TableView * table_ptr , size_t column_ndx, size_t value)
{   
    return  table_ptr->find_first_bool(column_ndx,size_t_to_bool(value));
}

//size_t         find_first_bool(size_t column_ndx, bool value) const;
TIGHTDB_C_CS_API size_t tableview_find_first_int(TableView * table_ptr , size_t column_ndx, int64_t value)
{   
    return table_ptr->find_first_int(column_ndx,value);
}



TIGHTDB_C_CS_API tightdb::TableView* table_find_all_int(Table * table_ptr , size_t column_ndx, int64_t value)
{   
    return new TableView(table_ptr->find_all_int(column_ndx,value));            
}

TIGHTDB_C_CS_API tightdb::TableView* table_distinct(Table * table_ptr , size_t column_ndx)
{   
    return new TableView(table_ptr->distinct(column_ndx));
}

//currently only legal with string columns
TIGHTDB_C_CS_API void table_set_index(Table * table_ptr , size_t column_ndx)
{   
    table_ptr->set_index(column_ndx);
}


//convert from columnName to columnIndex returns -1 if the string is not a column name
//assuming that the get_table() does not return anything that must be deleted
TIGHTDB_C_CS_API size_t query_get_column_index(tightdb::Query* query_ptr,char *  column_name)
{
    return query_ptr->get_table()->get_column_index(column_name);
}

//todo:implement call that uses all the parametres
TIGHTDB_C_CS_API double query_average(tightdb::Query* query_ptr,size_t column_index)
{
    return query_ptr->average(column_index);//use default values for the defaultable parametres
}


TIGHTDB_C_CS_API size_t table_get_column_index(Table* table_ptr,char *  column_name)
{
    return table_ptr->get_column_index(column_name);
}

TIGHTDB_C_CS_API size_t tableview_get_column_index(TableView* tableView_ptr,char *  column_name)
{
    return tableView_ptr->get_column_index(column_name);
}



//    TableView      find_all(size_t start=0, size_t end=size_t(-1), size_t limit=size_t(-1));
TIGHTDB_C_CS_API tightdb::TableView* query_find_all(Query * query_ptr , size_t start, size_t end, size_t limit)
{   
    return new TableView( query_ptr->find_all(start,end,limit));
}


TIGHTDB_C_CS_API Query* table_where(Table * table_ptr)
{   
    return new Query(table_ptr->where());            
}

TIGHTDB_C_CS_API size_t query_find_next(Query * query_ptr, size_t last_match) 
{
    return query_ptr->find_next(last_match);
}


//query_bool_equal64(IntPtr queryPtr, IntPtr columnIndex,IntPtr value);

//todo:unit test both a false and a positive one
//the query_bool_equal will never return null
TIGHTDB_C_CS_API void query_bool_equal(Query * query_ptr, size_t columnIndex, size_t value)
{    
    query_ptr->equal(columnIndex,size_t_to_bool(value));    
    //return &(query_ptr->equal(columnIndex,size_t_to_bool(value)));//is this okay? will I get the address of a Query object that i can call in another pinvoke?
}

TIGHTDB_C_CS_API void tableview_delete(TableView * tableview_ptr )
{
//    std::cerr<<"before delete tableview_ptr \n";
    delete(tableview_ptr);
}

TIGHTDB_C_CS_API void query_delete(Query* query_ptr )
{ 
//    std::cerr<<"before delete query_ptr \n";
    delete(query_ptr);
}

TIGHTDB_C_CS_API void group_delete(Group* group_ptr )
{
  //  std::cerr<<"before delete group_ptr\n";
    delete(group_ptr);
}




TIGHTDB_C_CS_API int64_t table_get_int(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_int(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API int64_t tableview_get_int(TableView* tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableView_ptr->get_int(column_ndx,row_ndx);
}


//returns false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API size_t tableview_get_bool(TableView* tableView_ptr, size_t column_ndx, size_t row_ndx)
{    
    return bool_to_size_t( tableView_ptr->get_bool(column_ndx,row_ndx));
}


//call with false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API void tableview_set_bool(TableView* tableView_ptr, size_t column_ndx, size_t row_ndx,size_t value)
{    
     tableView_ptr->set_bool(column_ndx,row_ndx,size_t_to_bool(value));     
}


//returns false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API size_t table_get_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{    
    return bool_to_size_t(table_ptr->get_bool(column_ndx,row_ndx));
}

//call with false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API void table_set_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx,size_t value)
{    
     table_ptr->set_bool(column_ndx,row_ndx,size_t_to_bool(value));     
}

TIGHTDB_C_CS_API float table_get_float(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_float(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API float tableview_get_float(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableview_ptr->get_float(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API double table_get_double(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_double(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API double tableview_get_double(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableview_ptr->get_double(column_ndx,row_ndx);
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
   table_ptr->set_mixed(col_ndx,row_ndx,Mixed::subtable_tag());
}

TIGHTDB_C_CS_API void tableview_set_mixed_empty_subtable(TableView* tableView_ptr,size_t col_ndx, size_t row_ndx)
{     
   tableView_ptr->set_mixed(col_ndx,row_ndx,Mixed::subtable_tag());
}


TIGHTDB_C_CS_API size_t table_size(Table* table_ptr) 
{
    return table_ptr->size();
}


TIGHTDB_C_CS_API size_t tableview_size(TableView* tableview_ptr) 
{
    return tableview_ptr->size();
}


//these functions are used by unit tests and initialization to ensure that the current p/invoke layer is working as expected.
//what we check is : 
//1) that stack size matches for all types used in the p/invoke layer
//2) that all logical values are translated correctly C# to c++
//3) that all logical values are translated correctly c++ to c#
//4) that values send by c# to c++ can be returned again by c++ and not change
//These tests should report if there is a problem with a new c++ compiler, a new hardware platform , a new C# compiler, a new .net compatible platform, 
//changes by microsoft to the default marshalling behaviou, etc. etc.
//for instance the tests would reveal if a c++ binding was compiled with  _USE_32BIT_TIME_T which we would not support, as we expect time_t to always be 64bit size


//C# makes some assumptions reg. the size of a size_t, This call double-checks that C# is right about the size of size_t. 
//This call relies on the int32_t type being defined, if it is not, at least we get a compile time error
//it is assumed that sizeof(size_t) will not return a number larger than 2^32 (in fact, 4 or 8 is expected,but one day we might see 12 or 16, who knows)
//int32_t is mentioned here http://en.cppreference.com/w/cpp/types/integer 
TIGHTDB_C_CS_API int32_t test_sizeofsize_t()
{
    int32_t sizeofsize_t;
    sizeofsize_t = sizeof(size_t);//i hope the compiles manages to stuff the 64 bit size_t into the 32 bit int32_t when we are on a 64 bit platform
    return sizeofsize_t;
}

//this is defined to be always 4 bytes, so it probably will be
TIGHTDB_C_CS_API size_t test_sizeofint32_t()
{
    return sizeof(int32_t);
}

//defined to follow size of size_t so probably always will C# will simply check that sizeof(size_t) matches sizeof(Table*)
TIGHTDB_C_CS_API size_t test_sizeoftablepointer()
{
    return sizeof(Table*);
}

//should be defined by size_t so will return 4 or 8 unless something is really strange C# will test in the same way as with Table*
TIGHTDB_C_CS_API size_t test_sizeofcharpointer()
{
    return sizeof(char *);
}

//should be 64 bits always
TIGHTDB_C_CS_API size_t test_sizeofint64_t()
{
    return sizeof(int64_t);
}

//return the size of a float. Used by the C# binding unit test that ensures that at runtime, the float size we expect is also the one c++ sends
TIGHTDB_C_CS_API size_t test_sizeoffloat()
{
    return sizeof(float);
}

//return the size of a float. Used by the C# binding unit test that ensures that at runtime, the float size we expect is also the one c++ sends
TIGHTDB_C_CS_API size_t test_sizeofdouble()
{
    return sizeof(double);
}

//return the size of time_t. C# expects this to be 64 bits always, but it might be 32 bit on some compilers, a C# unit test will discover this by calling this function
//and getting 4 instead of 8
TIGHTDB_C_CS_API size_t test_sizeoftime_t()
{
    return sizeof(time_t);
}

//this test ensurese that parametres are put on the stack, and read from the stack in the same sequence
//expects the caller to call with parametres valued (1,2,3,4,5)
//never returns 0
//returns 1 if everything is okay, otherwise returns 1+the (1-based) position of the c++ parameter that contained an unexpected value, plus 10* its value
//so if parameter1 (expected to contain 1) contained 0, 1+1+0*10 is returned
//and if the sequence is not as expected, reveals what parameter had an unexpected value, and what that value was
TIGHTDB_C_CS_API size_t test_get_five_parametres(size_t input1,size_t input2,size_t input3, size_t input4, size_t input5)
{
    if(input1!=1)
        return 2+input1*10;
    if(input2!=2)
        return 2+input2*10;
    if(input3!=3)
        return 3+input3*10;
    if(input4!=4)
        return 4+input4*10;
    if(input5!=5)
        return 5+input5*10;    
    return 1;
}



//the following tests ensures that the C# types and the mapped c++ types cover the exact same range
TIGHTDB_C_CS_API size_t test_size_t_max()
{
    return std::numeric_limits<size_t>::max();
}

TIGHTDB_C_CS_API size_t test_size_t_min()
{
    return std::numeric_limits<size_t>::min();
}

//used to test that values can round-trip without being changed
TIGHTDB_C_CS_API size_t test_size_t_return(size_t input)
{
    return input;
}



//the following tests ensures that the C# types and the mapped c++ types cover the exact same range
TIGHTDB_C_CS_API float test_float_max()
{
    return std::numeric_limits<float>::max();
}

TIGHTDB_C_CS_API float test_float_min()
{
    return std::numeric_limits<float>::lowest();
}

//used to test that values can round-trip without being changed
TIGHTDB_C_CS_API float test_float_return(float input)
{
    return input;
}


//the following tests ensures that the C# types and the mapped c++ types cover the exact same range
TIGHTDB_C_CS_API double test_double_max()
{
    return std::numeric_limits<double>::max();
}

TIGHTDB_C_CS_API double test_double_min()
{
    return std::numeric_limits<double>::lowest();
}

//used to test that values can round-trip without being changed
TIGHTDB_C_CS_API double test_double_return(double input)
{
    return input;
}

//as C++ cannot return the highest allowed value of an enum, we cannot really test the logical enum values
//we could create a method for each enum, to see if it maps to the same value on the other side, like  get_type_int get_type_bool etc.
//then call them and check that the values are the expected ones. This will not, however, ensure that we discover if c++ gets a new one,that C# does not
//know about
/*
TIGHTDB_C_CS_API size_t test_datatype_max() {
    return (size_t)DataType.Max;
}
*/


//the following tests ensures that the C# types and the mapped c++ types cover the exact same range
TIGHTDB_C_CS_API int64_t test_int64_t_max()
{
    return std::numeric_limits<int64_t>::max();
}

TIGHTDB_C_CS_API int64_t test_int64_t_min()
{
    return std::numeric_limits<int64_t>::min();
}

//used to test that values can round-trip without being changed
TIGHTDB_C_CS_API int64_t test_int64_t_return(int64_t input)
{
    return input;
}


TIGHTDB_C_CS_API size_t test_return_datatype(size_t value) {
    return datatype_to_size_t(size_t_to_datatype(value));
}


TIGHTDB_C_CS_API size_t test_return_bool(size_t value) {
    return bool_to_size_t(size_t_to_bool(value));
}

TIGHTDB_C_CS_API size_t test_return_true_bool() {
    return bool_to_size_t(true);
}

TIGHTDB_C_CS_API size_t test_return_false_bool() {
    return bool_to_size_t(false);
}

TIGHTDB_C_CS_API int64_t test_increment_integer(int64_t value) {
    return value++;
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

