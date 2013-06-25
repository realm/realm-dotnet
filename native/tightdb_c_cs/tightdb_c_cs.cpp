// tightdb_c_cs.cpp : Defines the exported functions for the DLL application.
/*
    
Todo:Create unit tests that stresses tightdb with field names that contains all sorts of unicode caracters
that are valid in windows, but do not exist in ansi.  Also create cases with 16bit unicode characters with a zero in them
we should not break easily, and we should know where we have problems.

*/

#include "stdafx.hpp"
#include "tightdb_c_cs.hpp"
#include "tightdb/utf8.hpp"
#include <tightdb/unique_ptr.hpp>
#include <iostream>
#include <sstream>

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


//adapted from tightdb_java2/tightdb_jni/src/util.cpp. Used by utf8 to utf16 conversions


  class CSStringAccessor {
public:
    CSStringAccessor(uint16_t *, size_t);

    operator tightdb::StringData() const TIGHTDB_NOEXCEPT
    {
        return tightdb::StringData(m_data.get(), m_size);
    }
    bool error;
private:
    tightdb::UniquePtr<char[]> m_data;
    std::size_t m_size;
};


  
CSStringAccessor::CSStringAccessor(uint16_t* csbuffer, size_t csbufsize)
{
    // For efficiency, if the incoming UTF-16 string is sufficiently
    // small, we will choose an UTF-8 output buffer whose size (in
    // bytes) is simply 4 times the number of 16-bit elements in the
    // input. This is guaranteed to be enough. However, to avoid
    // excessive over allocation, this is not done for larger input
    // strings.
    
    error=false;
    typedef Utf8x16<uint16_t,std::char_traits<char16_t>>Xcode;    //This might not work in old compilers (the std::char_traits<char16_t> ).     
    size_t max_project_size = 48;

    TIGHTDB_ASSERT(max_project_size <= std::numeric_limits<size_t>::max()/4);

    size_t u8buf_size;
    if (csbufsize <= max_project_size) {
        u8buf_size = csbufsize * 4;
    }
    else {
        const uint16_t* begin = csbuffer;
        const uint16_t* end   = csbuffer + csbufsize;
        u8buf_size = Xcode::find_utf8_buf_size(begin, end);
    }
    m_data.reset(new char[u8buf_size]);
    {
        const uint16_t* in_begin = csbuffer;
        const uint16_t* in_end   = csbuffer + csbufsize;
        char* out_begin = m_data.get();
        char* out_end   = m_data.get() + u8buf_size;
        if (!Xcode::to_utf8(in_begin, in_end, out_begin, out_end)){
           m_size=0;
           error=true;
           return;//calling method should handle this. We can't throw exceptions
        }
        TIGHTDB_ASSERT(in_begin == in_end);
        m_size = out_begin - m_data.get();
    }
}


//stringdata is utf8
//cshapbuffer is a c# stringbuilder buffer marshalled as utf16 bufsize is the size of the csharp buffer measured in 16 bit words. The buffer is in fact one char larger than that, to make room for a terminating null character
//this method will transcode the utf8 string data inside stringdata to utf16 and put the transcoded data in the buffer. the return value is the size of the buffer that was
//actually used, measured in 16 bit characters, excluding a null terminator that is also put in
//if the return sizee is larger than bufsize_in_16bit_words, the buffer was too small, this is a request to be called again with a larger buffer
//note that this implementation will preserve null characters inside the string - but the C# interop marshalling stuff will truncate the string at the first null character anyways
//To get around that, we would have to work with an untyped pointer.

//possible return values :
//-1            :The utf8 data pointed to by str cannot be translated to utf16. it is invalid
//>=0;<=bufsize :The data in str has been converted to data in csharpbuffer - return value is number of 16 bit characters in cshapbuffer that contains the converted data
//>bufsize      :The buffer size is too small for the translated string. Please call again with a buffer of at least the size of the return value
size_t stringdata_to_csharpstringbuffer(StringData str, uint16_t * csharpbuffer, size_t bufsize) //note bufsize is _in_16bit_words 
{
    //fast check. If the buffer is very likely too small, just return immediatly with a request for a larger buffer
    if(str.size()>bufsize) {
        return str.size();
    }

    //fast check. Empty strings are handled by just returning zero, not even touching the buffer
    if(str.size()<=0) {
        return 0;
    }
    const char* in_begin = str.data();
    const char* in_end = str.data() +str.size();

    uint16_t* out_begin = csharpbuffer;    
    uint16_t* out_end = csharpbuffer+bufsize;
    
    typedef Utf8x16<uint16_t,std::char_traits<char16_t>>Xcode;    //This might not work in old compilers (the std::char_traits<char16_t> ). 
    
    size_t size  = Xcode::find_utf16_buf_size(in_begin,in_end);//Figure how much space is actually needed
    
    if(in_begin!=in_end) {
        std::cerr<<str.data();
      return -1;//bad uft8 data    
    }
    if(size>bufsize) 
        return size; //bufsize is too small. Return needed size
    
    //the transcoded string fits in the buffer

     in_begin = str.data();
     in_end = str.data() +str.size();

    if (Xcode::to_utf16(in_begin,in_end,out_begin,out_end))  {
        size_t chars_used =out_begin-csharpbuffer;
        //csharpbuffer[chars_used-5]=0; //slightly ugly hack. C# looks for a null terminated string in the buffer, so we have to null terminate this string for C# to pick up where the end is
        return (chars_used);        //transcode complete. return the number of 16-bit characters used in the buffer,excluding the null terminator
    }
    return -1;//bad utf8 data. this cannot happen
}


} //anonymous namespace

#ifdef __cplusplus
extern "C" {
#endif


// return a (manually changed) constant - used when debugging to manually ensure a newly compiled dll is being linked to


 TIGHTDB_C_CS_API size_t tightdb_c_cs_getver(void){

  // Table test;
	return 1306071446;
}

 //return a newly constructed top level table 
TIGHTDB_C_CS_API Table* new_table()//should be disposed by calling unbind_table_ref
{
	//return reinterpret_cast<size_t>(LangBindHelper::new_table());	 
	return LangBindHelper::new_table();
}


 TIGHTDB_C_CS_API Table* table_copy_table(tightdb::Table* table_ptr)
 {
     return LangBindHelper::copy_table(*table_ptr);
 }

 TIGHTDB_C_CS_API size_t table_is_valid(Table* table_ptr)
 {
     return size_t_to_bool(table_ptr->is_valid());
 }

//     bool has_shared_spec() const;
 TIGHTDB_C_CS_API size_t table_has_shared_spec(Table* table_ptr) {
	 return table_ptr->has_shared_spec();
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


TIGHTDB_C_CS_API  void table_update_from_spec(Table* table_ptr)
{
    table_ptr->update_from_spec();
}

TIGHTDB_C_CS_API size_t table_add_column(tightdb::Table* table_ptr,size_t type,  uint16_t * name,size_t name_len)
{
    CSStringAccessor str(name,name_len);
    return table_ptr->add_column(size_t_to_datatype(type),str);
}


//todo:implement size_t      add_subcolumn(const std::vector<size_t>& column_path, DataType type, StringData name);

TIGHTDB_C_CS_API void table_remove_column(Table* table_ptr, size_t column_ndx)
{
    table_ptr->remove_column(column_ndx);
}

//todo:implement     size_t      add_subcolumn(const std::vector<size_t>& column_path, DataType type, StringData name);

TIGHTDB_C_CS_API void table_rename_column(Table* table_ptr, size_t column_ndx, uint16_t* value, size_t value_len)
{
    CSStringAccessor str(value,value_len);
    table_ptr->rename_column(column_ndx,str);
}

//todo:implement     void        rename_column(const std::vector<size_t>& column_path, StringData name);

//todo:isempty is implemented in C# with a call to size. Perhaps better if we call c++ is_empty itself

TIGHTDB_C_CS_API size_t table_size(Table* table_ptr) 
{
    return table_ptr->size();
}

//implement   void        clear();

TIGHTDB_C_CS_API size_t table_get_column_count(tightdb::Table* table_ptr)
{
	return table_ptr->get_column_count();
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

TIGHTDB_C_CS_API size_t table_get_column_name(Table* table_ptr,size_t column_ndx,uint16_t * colname, size_t bufsize)
{
    StringData str= table_ptr->get_column_name(column_ndx);   
    return stringdata_to_csharpstringbuffer(str,colname,bufsize);
}


//convert from columnName to columnIndex returns -1 if the string is not a column name
//assuming that the get_table() does not return anything that must be deleted
TIGHTDB_C_CS_API size_t query_get_column_index(tightdb::Query* query_ptr,uint16_t *  column_name,size_t column_name_len)
{
     CSStringAccessor str(column_name,column_name_len);
    return query_ptr->get_table()->get_column_index(str);
}


//    DataType    get_column_type(size_t column_ndx) const TIGHTDB_NOEXCEPT;
TIGHTDB_C_CS_API  size_t table_get_column_type(Table* table_ptr, const size_t column_ndx)
{
	return datatype_to_size_t(table_ptr->get_column_type(column_ndx));
}

TIGHTDB_C_CS_API size_t table_add_empty_row(Table* table_ptr, size_t num_rows)
{   
    return table_ptr->add_empty_row(num_rows);
}


TIGHTDB_C_CS_API void table_insert_empty_row(Table* table_ptr, size_t row_ndx, size_t num_rows)
{   
    table_ptr->insert_empty_row(row_ndx,num_rows);
}

TIGHTDB_C_CS_API void table_remove_row(tightdb::Table* table_ptr, size_t row_ndx)
{
    table_ptr->remove(row_ndx);
}

//todo:implement remove_last

//todo:implement move_last_over

//todo:implement the insert interface to be used not by users, but by the binding itself to speed up
//whole row inserts

TIGHTDB_C_CS_API void table_insert_int(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    table_ptr->insert_int(column_ndx,row_ndx,value);
}


TIGHTDB_C_CS_API int64_t table_get_int(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_int(column_ndx,row_ndx);
}

//returns false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API size_t table_get_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{    
    return bool_to_size_t(table_ptr->get_bool(column_ndx,row_ndx));
}

TIGHTDB_C_CS_API int64_t table_get_date(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
    return date_to_int64_t(table_ptr->get_date(column_ndx,row_ndx));
}

TIGHTDB_C_CS_API float table_get_float(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_float(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API double table_get_double(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_double(column_ndx,row_ndx);
}

//todo:Csharp will currently treat the returned data as ansi with the current codepage - this will be fixed at the same time as the new tightdb strings are released,
//as the conversion from utc-8 to utc-16 will be done on the c++ side
TIGHTDB_C_CS_API size_t table_get_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t * datatochsarp, size_t bufsize)
{
    StringData fielddata=table_ptr->get_string(column_ndx, row_ndx);
    return stringdata_to_csharpstringbuffer(fielddata, datatochsarp,bufsize);
}


//todo:implement table get binary      BinaryData  get_binary(size_t column_ndx, size_t row_ndx) const; // FIXME: Should be modified so it never throws



//hertil



TIGHTDB_C_CS_API size_t tableview_get_column_count(tightdb::TableView* tableView_ptr)
{
	return tableView_ptr->get_column_count();
}


//new empty group
//todo:This fails on windows, awaiting new version (problem is something with the filename, read/write rights where it is put by default)

TIGHTDB_C_CS_API Group* new_group() //should be disposed by calling group_delete
{
//    std::cerr<<"before new group()\n";
    //works Group* g = new Group(Group::unattached_tag());
    //fails  Group* g = new Group();
    Group* g = new Group();
//      std::cerr<<"after new group()\n";
    return g;
//    return new Group();        
}





  TIGHTDB_C_CS_API Group* new_group_file(uint16_t * name, size_t name_len)//should be disposed by calling group_delete
{  
    CSStringAccessor name2(name,name_len);
    Group* g = new Group(StringData(name2));
//    std::cerr<<"Message from c++. Group created. address: ("<<g <<") filename ("<<name2<<")\n";
    return g;
}

//write group to specified file
TIGHTDB_C_CS_API size_t group_write(Group* group_ptr,uint16_t * name, size_t name_len)

{   
    try {
    CSStringAccessor str(name,name_len);    
    group_ptr->write(StringData(str));
    return 0;//0 means no exception thrown
    }
    //if the file is already there, or other file related trouble
   catch (File::AccessError) {             
       return 1;//1 means IO problem exception was thrown. C# always use IOException in cases like this anyways so no need to detail it out further
   }
}


//should be disposed by calling unbind_table_ref
TIGHTDB_C_CS_API Table* group_get_table(Group* group_ptr,uint16_t* table_name,size_t table_name_len)
{   
    CSStringAccessor str(table_name,table_name_len);
    return LangBindHelper::get_table_ptr(group_ptr,str);
}


TIGHTDB_C_CS_API size_t group_has_table(Group* group_ptr, uint16_t * table_name,size_t table_name_len)//should be disposed by calling unbind_table_ref
{    
    CSStringAccessor str(table_name,table_name_len);
    return bool_to_size_t(group_ptr->has_table(str));
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


TIGHTDB_C_CS_API void table_clear_subtable(Table* table_ptr, size_t column_ndx, size_t row_ndx) 
{
    table_ptr->clear_subtable(column_ndx,row_ndx);
}

TIGHTDB_C_CS_API void tableview_clear_subtable(TableView* tableView_ptr, size_t column_ndx, size_t row_ndx) 
{
    tableView_ptr->clear_subtable(column_ndx,row_ndx);
}


TIGHTDB_C_CS_API void unbind_table_ref(tightdb::Table* table_ptr)
{	
	LangBindHelper::unbind_table_ref(table_ptr);
}


TIGHTDB_C_CS_API void tableview_remove_row(tightdb::TableView* tableView_ptr, size_t row_ndx)
{
    tableView_ptr->remove(row_ndx);
}

/*

//the value returned is the number of chars that are in use in the fromcppbuffer
TIGHTDB_C_CS_API size_t test_string_returner(uint16_t* tocppbuffer, size_t tocppbuffersize, uint16_t* fromcppbuffer,size_t fromcppbuffersize) 
{
    CSStringAccessor CSString(tocppbuffer,tocppbuffersize);//acquire the cs string , convert to UTF-8 and put it into CSString
    StringData fromcs = CSString;//create a StringData wrapper around the string
    return stringdata_to_csharpstringbuffer(fromcs,fromcppbuffer,fromcppbuffersize);//fill up the return buffer with the UTF-16 version of the UTF-8 fromcs
}

*/


//    size_t add_column(DataType type, const char* name, ColumnType attr=col_attr_None);
//note that we have omitted support for attr until we figure what it's for
TIGHTDB_C_CS_API size_t spec_add_column(Spec* spec_ptr,size_t type, uint16_t * name,size_t name_len) 
{
    CSStringAccessor str(name,name_len);
	return spec_ptr->add_column(size_t_to_datatype(type),str);		
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


TIGHTDB_C_CS_API  size_t spec_get_column_type(Spec* spec_ptr, const size_t column_ndx)
{
   
    return datatype_to_size_t(spec_ptr->get_column_type(column_ndx));
}
















//returns 42 if we got "Hellow, World!" otherwise return -42
TIGHTDB_C_CS_API size_t test_string_to_cpp(uint16_t * str,size_t bufsize)
{
    CSStringAccessor CSString(str,bufsize);
    //csharpstringbuffer_to_stringdata(stringdata, str,bufsize);
    StringData sd=CSString;
    if (sd==("Hello, World!")) {
        return 42;
    }
    std::cerr<<sd;
    return -42;    
}




//the value returned is the number of chars that are in use in the fromcppbuffer
TIGHTDB_C_CS_API size_t test_string_returner(uint16_t* tocppbuffer, size_t tocppbuffersize, uint16_t* fromcppbuffer,size_t fromcppbuffersize) 
{
    CSStringAccessor CSString(tocppbuffer,tocppbuffersize);//acquire the cs string , convert to UTF-8 and put it into CSString
    StringData fromcs = CSString;//create a StringData wrapper around the string
    return stringdata_to_csharpstringbuffer(fromcs,fromcppbuffer,fromcppbuffersize);//fill up the return buffer with the UTF-16 version of the UTF-8 fromcs
}

TIGHTDB_C_CS_API size_t test_string_from_cpp(uint16_t * buffer, size_t bufsize)
{
    StringData str = StringData("Hello, World!");
    return stringdata_to_csharpstringbuffer(str,buffer, bufsize);    
}



TIGHTDB_C_CS_API size_t tableview_get_string(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx, uint16_t * datatocsharp, size_t bufsize)
{
    StringData fielddata=tableview_ptr->get_string(column_ndx, row_ndx);
    return stringdata_to_csharpstringbuffer(fielddata,datatocsharp,bufsize);
}

TIGHTDB_C_CS_API size_t tableview_get_column_name(TableView* tableView_ptr,size_t column_ndx,uint16_t * colname, size_t bufsize)
{
    StringData cn= tableView_ptr->get_column_name(column_ndx);
    return stringdata_to_csharpstringbuffer(cn, colname,bufsize);
}

TIGHTDB_C_CS_API size_t spec_get_column_name(Spec* spec_ptr,size_t column_ndx,uint16_t * colname, size_t bufsize)
{
	StringData cn= spec_ptr->get_column_name(column_ndx);
    return stringdata_to_csharpstringbuffer(cn,colname,bufsize);
}

//    Spec add_subtable_column(const char* name);
//todo:utf16 to utf8 the name parameter.
TIGHTDB_C_CS_API Spec* spec_add_subtable_column(Spec* spec_ptr,uint16_t* name,size_t name_len)//the returned spec should be disposed of by calling spec_deallocate
{	
    CSStringAccessor str(name,name_len);
	Spec subtablespec = spec_ptr->add_subtable_column(str);//will add_subtable_column return the address to a spec?
	return new Spec(subtablespec);
}


TIGHTDB_C_CS_API void table_set_mixed_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t* value,size_t value_len)
{
    CSStringAccessor str(value,value_len);
    StringData strd = str;
    table_ptr->set_mixed(column_ndx,row_ndx,strd);
}

TIGHTDB_C_CS_API void tableview_set_mixed_string(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx, uint16_t* value,size_t value_len)
{
    CSStringAccessor str(value,value_len);
    StringData strd = str;
    tableview_ptr->set_mixed(column_ndx,row_ndx,strd);
}

TIGHTDB_C_CS_API void table_set_string(Table* table_ptr, size_t column_ndx, size_t row_ndx,uint16_t* value,size_t value_len)
{
    CSStringAccessor str(value,value_len);
    table_ptr->set_string(column_ndx,row_ndx,str);
}

TIGHTDB_C_CS_API void tableview_set_string(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx,uint16_t* value,size_t value_len)
{
    CSStringAccessor str (value,value_len);
    tableview_ptr->set_string(column_ndx,row_ndx,str);
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
    table_ptr->set_mixed(column_ndx,row_ndx,int64_t_to_date(value));
}

TIGHTDB_C_CS_API int64_t tableview_get_mixed_date(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return date_to_int64_t(tableView_ptr->get_mixed(column_ndx,row_ndx).get_date());
}

TIGHTDB_C_CS_API int64_t table_get_mixed_date(Table*  table_ptr, size_t column_ndx, size_t row_ndx)
{
   return date_to_int64_t(table_ptr->get_mixed(column_ndx,row_ndx).get_date());    
}

TIGHTDB_C_CS_API int64_t tableview_get_date(TableView*  tableView_ptr, size_t column_ndx, size_t row_ndx)
{
    return date_to_int64_t(tableView_ptr->get_date(column_ndx,row_ndx));
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

TIGHTDB_C_CS_API size_t table_find_first_binary(Table * table_ptr , size_t column_ndx, char * value, size_t len)
{   
    BinaryData bd = BinaryData(value,len);
    return  table_ptr->find_first_binary(column_ndx,bd);
}


TIGHTDB_C_CS_API size_t table_find_first_string(Table * table_ptr , size_t column_ndx, uint16_t * value,size_t value_len)
{   
    CSStringAccessor str(value,value_len);
    return  table_ptr->find_first_string(column_ndx,str);
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

TIGHTDB_C_CS_API size_t tableview_find_first_string(TableView * table_ptr , size_t column_ndx, uint16_t* value,size_t value_len)
{   
    CSStringAccessor str(value,value_len);
    return  table_ptr->find_first_string(column_ndx,str);
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

TIGHTDB_C_CS_API tightdb::TableView* table_find_all_string(Table * table_ptr , size_t column_ndx, uint16_t* value,size_t value_len)
{   
    CSStringAccessor str(value,value_len);
    return new TableView(table_ptr->find_all_string(column_ndx,str));            
}

TIGHTDB_C_CS_API tightdb::TableView* tableview_find_all_int(TableView * tableview_ptr , size_t column_ndx, int64_t value)
{   
    return new TableView(tableview_ptr->find_all_int(column_ndx,value));            
}

TIGHTDB_C_CS_API tightdb::TableView* tableview_find_all_string(TableView * tableview_ptr , size_t column_ndx, uint16_t* value,size_t value_len)
{   
    CSStringAccessor str(value,value_len);
    return new TableView(tableview_ptr->find_all_string(column_ndx,str));
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


TIGHTDB_C_CS_API int64_t table_count_int(Table * table_ptr , size_t column_ndx,int64_t target)
{   
    return table_ptr->count_int(column_ndx,target);
}

TIGHTDB_C_CS_API int64_t table_count_string(Table * table_ptr , size_t column_ndx,uint16_t * target,size_t target_len)
{   
    CSStringAccessor str(target,target_len);
    return table_ptr->count_string(column_ndx,str);    
}

TIGHTDB_C_CS_API int64_t table_count_float(Table * table_ptr , size_t column_ndx,float target)
{   
    return table_ptr->count_float(column_ndx,target);
}

TIGHTDB_C_CS_API int64_t table_count_double(Table * table_ptr , size_t column_ndx,double target)
{   
    return table_ptr->count_double(column_ndx,target);
}





TIGHTDB_C_CS_API int64_t table_sum_int(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->sum(column_ndx);
}
TIGHTDB_C_CS_API double table_sum_float(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->sum_float(column_ndx);
}

TIGHTDB_C_CS_API double table_sum_double(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->sum_double(column_ndx);
}

TIGHTDB_C_CS_API int64_t table_maximum_int(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->maximum(column_ndx);
}
TIGHTDB_C_CS_API float table_maximum_float(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->maximum_float(column_ndx);
}

TIGHTDB_C_CS_API double table_maximum_double(Table * table_ptr , size_t column_ndx)
{   
    std::cerr<<"table_maximum_double"<<column_ndx <<" \n";
    return table_ptr->maximum_double(column_ndx);
}

TIGHTDB_C_CS_API int64_t table_minimum_int(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->minimum(column_ndx);
}
TIGHTDB_C_CS_API float table_minimum_float(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->minimum_float(column_ndx);
}
TIGHTDB_C_CS_API double table_minimum_double(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->minimum_double(column_ndx);
}

TIGHTDB_C_CS_API double table_average_int(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->average(column_ndx);
}
TIGHTDB_C_CS_API double table_average_float(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->average_float(column_ndx);
}
TIGHTDB_C_CS_API double table_average_double(Table * table_ptr , size_t column_ndx)
{   
    return table_ptr->average_double(column_ndx);
}



//these 4 are not implemented in tightdb yet, but when they are ready, just uncomment and test
TIGHTDB_C_CS_API int64_t tableview_count_int(TableView * tableview_ptr , size_t column_ndx,int64_t target)
{   
//    return tableview_ptr->count_int(column_ndx,target);
    return 0;
}

//waiting for this to be implemented in tightdb c++ bindings/core
TIGHTDB_C_CS_API int64_t tableview_count_string(TableView * tableview_ptr , size_t column_ndx,uint16_t * target,size_t target_len)
{   
//    CSStringAccessor str(target,target_len);
//    return tableview_ptr->count_string(column_ndx,str);  
    return 0;
}

TIGHTDB_C_CS_API int64_t tableview_count_float(TableView * tableview_ptr , size_t column_ndx,float target)
{   
//    return tableview_ptr->count_float(column_ndx,target);
        return 0;
}

TIGHTDB_C_CS_API int64_t tableview_count_double(TableView * tableview_ptr , size_t column_ndx,double target)
{   
//    return tableview_ptr->count_double(column_ndx,target);
        return 0;
}



TIGHTDB_C_CS_API int64_t tableview_sum_int(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->sum(column_ndx);
}
TIGHTDB_C_CS_API double tableview_sum_float(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->sum_float(column_ndx);
}
TIGHTDB_C_CS_API double tableview_sum_double(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->sum_double(column_ndx);
}


TIGHTDB_C_CS_API int64_t tableview_maximum_int(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->maximum(column_ndx);
}
TIGHTDB_C_CS_API float tableview_maximum_float(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->maximum_float(column_ndx);
}
TIGHTDB_C_CS_API double tableview_maximum_double(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->maximum_double(column_ndx);
}

TIGHTDB_C_CS_API int64_t tableview_minimum_int(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->minimum(column_ndx);
}
TIGHTDB_C_CS_API float tableview_minimum_float(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->minimum_float(column_ndx);
}
TIGHTDB_C_CS_API double tableview_minimum_double(TableView * tableview_ptr , size_t column_ndx)
{   
    return tableview_ptr->minimum_double(column_ndx);
}



//these methods are not implemented in tightdb yet, when they are ready, remove the comments and test
TIGHTDB_C_CS_API double tableview_average_int(TableView * tableview_ptr , size_t column_ndx)
{   
//    return tableview_ptr->average(column_ndx);
    return 0;
}
TIGHTDB_C_CS_API double tableview_average_float(TableView * tableview_ptr , size_t column_ndx)
{   
//    return tableview_ptr->average_float(column_ndx);
        return 0;
}
TIGHTDB_C_CS_API double tableview_average_double(TableView * tableview_ptr , size_t column_ndx)
{   
//    return tableview_ptr->average_double(column_ndx);
        return 0;
}




//multiple issues with this one
//decide wether to implement an endpoint C# stream that then reads from the c++ stream output from to_json
//or just to put the to_json(ss) output into a std:string and then convert it to utf-16 and return it as a normal c# string
//the latter is of course more simple
//note that calling from C# it is probably best to guess the buffer size large enough, as the alternative is that the tightdb to_json method is called twice
//we could also create a class holding the string, and return a handle to it to C# and have another method that copies data once the buffer is large enough
TIGHTDB_C_CS_API size_t table_to_json(Table* table_ptr,uint16_t * data, size_t bufsize)
{
   // Write table to string in JSON format
   std::ostringstream ss;
   ss.sync_with_stdio(false); // for performance
   table_ptr->to_json(ss);   
   StringData str = ss.str(); 
   return stringdata_to_csharpstringbuffer(str,data,bufsize);
}


//todo:implement call that uses all the parametres
TIGHTDB_C_CS_API double query_average(tightdb::Query* query_ptr,size_t column_index)
{
    return query_ptr->average(column_index);//use default values for the defaultable parametres
}

TIGHTDB_C_CS_API size_t query_count(tightdb::Query* query_ptr,size_t start,size_t end,size_t limit)
{
    return query_ptr->count(start,end,limit);//use default values for the defaultable parametres
}

//caller should return wrappers pointer
TIGHTDB_C_CS_API void query_int_greater(tightdb::Query* query_ptr,size_t column_Index,int64_t value)
{
    query_ptr->greater(column_Index,value);
}



TIGHTDB_C_CS_API size_t table_get_column_index(Table* table_ptr,uint16_t *  column_name,size_t column_name_len)
{
    CSStringAccessor str = CSStringAccessor(column_name,column_name_len);
    return table_ptr->get_column_index(str);
}

TIGHTDB_C_CS_API size_t tableview_get_column_index(TableView* tableView_ptr,uint16_t *  column_name,size_t column_name_len)
{
    CSStringAccessor str = CSStringAccessor(column_name,column_name_len);
    return tableView_ptr->get_column_index(str);
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

TIGHTDB_C_CS_API void query_int_between(Query * query_ptr, size_t columnIndex, int64_t lowValue, int64_t highValue)

{    
    query_ptr->between(columnIndex,lowValue,highValue);    
}



TIGHTDB_C_CS_API void tableview_delete(TableView * tableview_ptr )
{
    delete(tableview_ptr);
}

TIGHTDB_C_CS_API void query_delete(Query* query_ptr )
{ 
    delete(query_ptr);
}

TIGHTDB_C_CS_API void group_delete(Group* group_ptr )
{
//    std::cerr<<"Message from c++. Group to be deleted. address: ("<<group_ptr <<")\n";
//    std::cerr<<group_ptr->size()<<"\n";//use g
    
    delete(group_ptr);
//    std::cerr<<"deleted group "<<group_ptr;
}


void test_test_test() {
     uint16_t *  test =(uint16_t * ) L"C:\\Develope\\Testgroupf";
    
    size_t namelen=22;
//    Group * g  = new_group_file(test,namelen);
    Group * g = new_group();
//        Group * g  = new_group_file(test,namelen);
//    Group* g = reinterpret_cast<Group*>(new int);
    g->get_table("hep");

    std::cerr<<g->size();//use g

    group_delete(g);
}


  TIGHTDB_C_CS_API void test_testacquireanddeletegroup(uint16_t * name, size_t len)
  
  {
//    std::cerr<<"Message from c++  callling test_test_test \n";

      test_test_test();
    std::cerr<<"Message from c++  call received with len "<<len<<"\n";
    Group* g = new_group_file(name,len);
    std::cerr<<"Message from c++  After call to new_group_file. g is("<<g<<") \n";
    group_delete(g);
    std::cerr<<"Message from c++  After call to group_delete";
//      Group* g  =  new Group("test");     
//	delete(g);
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

//call with false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API void tableview_set_mixed_bool(TableView* tableView_ptr, size_t column_ndx, size_t row_ndx,size_t value)
{    
     tableView_ptr->set_mixed(column_ndx,row_ndx,size_t_to_bool(value));     
}

//call with false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API void table_set_mixed_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx,size_t value)
{    
     table_ptr->set_mixed(column_ndx,row_ndx,size_t_to_bool(value));     
}



//call with false=0  true=1 we use a size_t as it is likely the fastest type to return
TIGHTDB_C_CS_API void table_set_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx,size_t value)
{    
     table_ptr->set_bool(column_ndx,row_ndx,size_t_to_bool(value));     
}



TIGHTDB_C_CS_API float tableview_get_float(TableView* tableview_ptr, size_t column_ndx, size_t row_ndx)
{
    return tableview_ptr->get_float(column_ndx,row_ndx);
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

