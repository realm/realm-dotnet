#include "wrappers.h"

#include <realm.hpp>
#include <realm/util/utf8.hpp>
#include <realm/lang_bind_helper.hpp>
#include <exception>
#include <string>

using namespace realm;

#ifdef WIN32
#define REALM_CORE_WRAPPER_API __declspec( dllexport )
#else
#define REALM_CORE_WRAPPER_API
#endif


using ManagedExceptionThrowerT = void(*)(size_t exceptionCode, void* utf8Str, size_t strLen);

// CALLBACK TO THROW IN MANAGED SPACE
static ManagedExceptionThrowerT ManagedExceptionThrower = nullptr;

extern "C" void set_exception_thrower(ManagedExceptionThrowerT userThrower)
{
    ManagedExceptionThrower = userThrower;
}


//as We've got no idea how the compiler represents an instance of DataType on the stack, perhaps it's better to send back a size_t with the value.
//we always know the size of a size_t
inline DataType size_t_to_datatype(size_t value){
  return (DataType)value;//todo:ask if this is a valid typecast. Or would it be better to use e.g int64? or reintepret_cast
}

//the followng functions convert to/from the types that we know have these features :
//* No marshalling involved - the transfer is fast
//* Blittable to C# types - the transfer is done without changes to the values (fast)
//* Types, that does not change on the c++ side between compileres and platforms
//* Types that have a mirror C# type that behaves the same way on different platforms (like IntPtr and size_t)

//bool is stored differently on different c++ compilers so use a size_t instead when p/invoking
inline bool size_t_to_bool(size_t value) 
{
  return value==1;//here i assume 1 and size_t can be compared in a meaningfull way. C# sends a size_t = 1 when true,and =0 when false
}

//send 1 for true, 0 for false.
//this function is compatible with the error checking functions in C#
//so You can send with this one, and check with an error checking one in C#
//useful if Your method has several exit paths, some of which are erorr conditions
inline size_t bool_to_size_t(bool value) {
  if(value) return 1;
  return 0;
}

//call this if something went wrong and You want to return an error code where C#
//expects a boolean or error code.
//the inline should end up with no more code than just returning the constant
//but will allow us to adopt another scheme later on
inline size_t bool_to_size_t_with_errorcode(size_t errorcode){
    return errorcode;
}

//a size_t sent from C# with value 0 means durability_full, other values means durabillity_memonly, but please
//use 1 for durabillity_memonly to make room for later extensions
inline SharedGroup::DurabilityLevel size_t_to_durabilitylevel(size_t value) {
    if (value==0) 
        return SharedGroup::durability_Full;
    return SharedGroup::durability_MemOnly;
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

  typedef realm::util::Utf8x16<uint16_t,std::char_traits<char16_t>>Xcode;    //This might not work in old compilers (the std::char_traits<char16_t> ). 

  size_t size  = Xcode::find_utf16_buf_size(in_begin,in_end);//Figure how much space is actually needed

  if(in_begin!=in_end) {
    std::cerr<<"BAD UTF8 DATA IN stringdata_tocsharpbuffer :"<<str.data()<<"\n";
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




class CSStringAccessor {
  public:
    CSStringAccessor(uint16_t *, size_t);

    operator realm::StringData() const //ASD has this vanished from core? REALM_NOEXCEPT
    {
      return realm::StringData(m_data.get(), m_size);
    }
    bool error;
  private:
    std::unique_ptr<char[]> m_data;
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
  typedef realm::util::Utf8x16<uint16_t,std::char_traits<char16_t>>Xcode;    //This might not work in old compilers (the std::char_traits<char16_t> ).     
  size_t max_project_size = 48;

  REALM_ASSERT(max_project_size <= std::numeric_limits<size_t>::max()/4);

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
    REALM_ASSERT(in_begin == in_end);
    m_size = out_begin - m_data.get();
  }
}


#ifdef __cplusplus
extern "C" {
#endif

#pragma region version  // {{{

  REALM_CORE_WRAPPER_API size_t realm_get_wrapper_ver()
  {
    return 20150616;
  }

  REALM_CORE_WRAPPER_API int realm_get_ver_minor()
  {
    return realm::Version::get_minor();
  }

#pragma endregion // }}}

#pragma region table // {{{

  //return a newly constructed top level table 
  REALM_CORE_WRAPPER_API Table* new_table()//should be disposed by calling unbind_table_ref
  {
    //return reinterpret_cast<size_t>(LangBindHelper::new_table());	 
    return LangBindHelper::new_table();
  }

  REALM_CORE_WRAPPER_API void table_unbind(Table* table_ptr)
  {
    LangBindHelper::unbind_table_ptr(table_ptr);
  }

  REALM_CORE_WRAPPER_API size_t table_add_column(realm::Table* table_ptr,size_t type,  uint16_t * name,size_t name_len)
  {
    CSStringAccessor str(name,name_len);
    return table_ptr->add_column(size_t_to_datatype(type),str);
  }

  REALM_CORE_WRAPPER_API Row* table_add_empty_row(Table* table_ptr)
  {   
    size_t row_ndx = table_ptr->add_empty_row(1);
    return new Row((*table_ptr)[row_ndx]);
    //return table_ptr->add_empty_row(num_rows);
  }

  //returns false=0  true=1 we use a size_t as it is likely the fastest type to return
  REALM_CORE_WRAPPER_API size_t table_get_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx)
  {
      return bool_to_size_t(table_ptr->get_bool(column_ndx, row_ndx));
  }

  REALM_CORE_WRAPPER_API int64_t table_get_int64(Table* table_ptr, size_t column_ndx, size_t row_ndx)
  {
      return table_ptr->get_int(column_ndx, row_ndx);
  }

  REALM_CORE_WRAPPER_API size_t table_get_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t * datatochsarp, size_t bufsize)
  {
    StringData fielddata=table_ptr->get_string(column_ndx, row_ndx);
    return stringdata_to_csharpstringbuffer(fielddata, datatochsarp,bufsize);
  }

  //call with false=0  true=1 we use a size_t as it is likely the fastest type to return
  REALM_CORE_WRAPPER_API void table_set_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx, size_t value)
  {
      table_ptr->set_bool(column_ndx, row_ndx, size_t_to_bool(value));
  }

  REALM_CORE_WRAPPER_API void table_set_int64(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
  {
      table_ptr->set_int(column_ndx, row_ndx, value);
  }

  REALM_CORE_WRAPPER_API void table_set_string(Table* table_ptr, size_t column_ndx, size_t row_ndx,uint16_t* value,size_t value_len)
  {
    CSStringAccessor str(value,value_len);
    table_ptr->set_string(column_ndx,row_ndx,str);
  }

  REALM_CORE_WRAPPER_API Query* table_where(Table* table_ptr)
  {   
    return new Query(table_ptr->where());            
  }

  REALM_CORE_WRAPPER_API size_t table_get_column_index(Table* table_ptr, uint16_t *  column_name, size_t column_name_len)
  {
      CSStringAccessor str = CSStringAccessor(column_name, column_name_len);
      return table_ptr->get_column_index(str);
  }

  REALM_CORE_WRAPPER_API size_t tableview_get_column_index(TableView* tableView_ptr, uint16_t *  column_name, size_t column_name_len)
  {
      CSStringAccessor str = CSStringAccessor(column_name, column_name_len);
      return tableView_ptr->get_column_index(str);
  }

  REALM_CORE_WRAPPER_API void table_remove_row(Table* table_ptr, Row* row_ptr)
  {
    table_ptr->move_last_over(row_ptr->get_index());
  }

#pragma endregion // }}}

#pragma region row // {{{

  REALM_CORE_WRAPPER_API void row_delete(Row* row_ptr)
  {
    delete row_ptr;
  }

  REALM_CORE_WRAPPER_API size_t row_get_row_index(Row* row_ptr)
  {
    return row_ptr->get_index();
  }

  REALM_CORE_WRAPPER_API size_t row_get_is_attached(Row* row_ptr)
  {
    return bool_to_size_t(row_ptr->is_attached());
  }

#pragma endregion // }}}

#pragma region query general // {{{

  REALM_CORE_WRAPPER_API void query_delete(Query* query_ptr)
  {
    delete(query_ptr);
  }

  // TODO: Replace this with TableView.
  REALM_CORE_WRAPPER_API Row* query_find(Query * query_ptr, size_t begin_at_table_row) 
  {
    if (begin_at_table_row >= query_ptr->get_table()->size())
      return nullptr;

    size_t row_ndx = query_ptr->find(begin_at_table_row);
    
    if (row_ndx == not_found)
      return nullptr;

    return new Row((*query_ptr->get_table())[row_ndx]);
  }

  //convert from columnName to columnIndex returns -1 if the string is not a column name
  //assuming that the get_table() does not return anything that must be deleted
  REALM_CORE_WRAPPER_API size_t query_get_column_index(Query* query_ptr,uint16_t *  column_name,size_t column_name_len)
  {
    CSStringAccessor str(column_name,column_name_len);
    return query_ptr->get_table()->get_column_index(str);
  }

#pragma endregion // }}}

#pragma region query group // {{{

  REALM_CORE_WRAPPER_API void query_group_begin(Query * query_ptr)
  {
      query_ptr->group();
  }

  REALM_CORE_WRAPPER_API void query_group_end(Query * query_ptr)
  {
      query_ptr->end_group();
  }

  REALM_CORE_WRAPPER_API void query_or(Query * query_ptr)
  {
      query_ptr->Or();
  }

#pragma endregion // }}}

#pragma region query string // {{{

  REALM_CORE_WRAPPER_API void query_string_equal(Query * query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
  {
      CSStringAccessor str(value, value_len);
      query_ptr->equal(columnIndex, str);
  }

  REALM_CORE_WRAPPER_API void query_string_not_equal(Query * query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
  {
      CSStringAccessor str(value, value_len);
      query_ptr->not_equal(columnIndex, str);
  }

#pragma endregion // }}}

#pragma region query bool // {{{
  REALM_CORE_WRAPPER_API void query_bool_equal(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->equal(columnIndex, size_t_to_bool(value));
  }

  REALM_CORE_WRAPPER_API void query_bool_not_equal(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->not_equal(columnIndex, size_t_to_bool(value));
  }

#pragma endregion // }}}


#pragma region query int // {{{
  REALM_CORE_WRAPPER_API void query_int_equal(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->equal(columnIndex, static_cast<int>(value));
  }

  REALM_CORE_WRAPPER_API void query_int_not_equal(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->not_equal(columnIndex, static_cast<int>(value));
  }

  REALM_CORE_WRAPPER_API void query_int_less(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->less(columnIndex, static_cast<int>(value));
  }

  REALM_CORE_WRAPPER_API void query_int_less_equal(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->less_equal(columnIndex, static_cast<int>(value));
  }

  REALM_CORE_WRAPPER_API void query_int_greater(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->greater(columnIndex, static_cast<int>(value));
  }

  REALM_CORE_WRAPPER_API void query_int_greater_equal(Query * query_ptr, size_t columnIndex, size_t value)
  {
      query_ptr->greater_equal(columnIndex, static_cast<int>(value));
  }

#pragma endregion // }}}


#pragma region query float // {{{
#pragma endregion // }}}


#pragma region query double // {{{
#pragma endregion // }}}



#pragma region group // {{{

//GROUP IMPLEMENTATION

//C# will call with a pinned array of bytes and its size. no copying occours except inside tightdb where the data is of course
//changed into a newly created group. The data pointed to by data must not be accessed after this call is finished, as C# will
//deallocate it again as soon as the call returns
//NOTE THAT tHE GROUP RETURNED HERE MUST BE FREED BY CALLING GROUP_DELETE WHEN IT IS NOT USED ANYMORE BY C#
REALM_CORE_WRAPPER_API Group* group_from_binary_data(const char* data, std::size_t size)
{
    try {
      BinaryData bd(data,size);
      return new Group(bd,false);
    } 
    catch (...)
    {
        return NULL;
    }
}


REALM_CORE_WRAPPER_API Group* new_group() //should be disposed by calling group_delete
{
    return  new Group();    
}

REALM_CORE_WRAPPER_API void group_delete(Group* group_ptr )
{  
    delete(group_ptr);
}


/*
    enum OpenMode {
        /// Open in read-only mode. Fail if the file does not already exist.
        mode_ReadOnly,
        /// Open in read/write mode. Create the file if it doesn't exist.
        mode_ReadWrite,
        /// Open in read/write mode. Fail if the file does not already exist.
        mode_ReadWriteNoCreate
    };
*/

  REALM_CORE_WRAPPER_API Group* new_group_file(uint16_t * name, size_t name_len, size_t openMode)//should be disposed by calling group_delete
{      

    //no like taking an enum from C# now it is unknown what underlying type Group::OpenMode might have
    //but we know that on any concievable platform, interop and C# marshalling works with size_t
    //so we convert the size_t to a valid Group::OpenMode here.

    Group::OpenMode om=Group::mode_ReadOnly;//this is the default value,if openMode==0

    if (openMode==1){
        om=Group::mode_ReadWrite;
    } else if(openMode==2) {
        om=Group::mode_ReadWriteNoCreate;
    }

    //in effect. 1 gives ReadWrite, 2 gives ReadWriteNoCreate, anything else give ReadOnly

    try{
      CSStringAccessor name2(name,name_len);

      return new Group(StringData(name2), 0, om); 
    }

    catch (std::exception& ) {
        return NULL;
    }
    catch (...) {
        std::cerr<<"CPPDLL: something non exception caught - returning NULL\n";
        return NULL;
    }
}

//write group to specified file
REALM_CORE_WRAPPER_API size_t group_write(Group* group_ptr,uint16_t * name, size_t name_len)

{   
    try {
    CSStringAccessor str(name,name_len);    
    group_ptr->write(StringData(str));
    return 0;//0 means no exception thrown
    }
    //if the file is already there, or other file related trouble
   catch (...) {             
       return 1;//1 means IO problem exception was thrown. C# always use IOException in cases like this anyways so no need to detail it out further
   }
}

/// Write this database to a memory buffer.
///
/// Ownership of the returned buffer is transferred to the
/// caller. The memory will have been allocated using
/// std::malloc().
//  BinaryData write_to_mem() const;
// The caller should call group_write_to_mem_free with the pointer returned from group_write_to_mem
//function returns a pointer to the data.
REALM_CORE_WRAPPER_API const char * group_write_to_mem(Group*  group_ptr,  size_t* size)
{

    BinaryData bd=group_ptr->write_to_mem();
    *size = bd.size();
    return  bd.data();//pointer to all the data;
}

//must be called with a pointer that was returned by group_write_to_mem
//DO NOT CALL IF THAT POINTER RETURNED WAS NULL
REALM_CORE_WRAPPER_API void group_write_to_mem_free(char * binarydata_ptr){
    if(binarydata_ptr!=NULL)  {
     std::free(binarydata_ptr);
    }
}

REALM_CORE_WRAPPER_API size_t group_commit(Group* group_ptr){
try {
    group_ptr->commit();
    return 0;
}
 catch(...){
     return 1;
 }
}

REALM_CORE_WRAPPER_API size_t group_equals(Group* group_ptr1, Group* group_ptr2)
{
    try {
        return bool_to_size_t(*group_ptr1==*group_ptr2);//utilizing operator overload
    }
    catch(...){
        return bool_to_size_t_with_errorcode(-1);//will return error -1 to a C# function expecting a bool
    }	
}

//inequality is handled in the binding by negating equality and thus we save one interop entry, and linking in the code for !=


REALM_CORE_WRAPPER_API size_t group_to_string(Group* group_ptr,uint16_t * data, size_t bufsize,size_t limit)
{   
   std::ostringstream ss;
   group_ptr->to_string(ss);
   std::string str = ss.str();   
   return stringdata_to_csharpstringbuffer(str,data,bufsize);
}


//return packed size_t with errorcode or a encoded boolean
REALM_CORE_WRAPPER_API size_t  group_is_empty(Group* group_ptr) {
    try {
        return bool_to_size_t(group_ptr->is_empty());//if we don't get an exception things went well
    }
    catch(...)//things did not go well
    {
        return bool_to_size_t_with_errorcode(-1);//return an error code to indicate this
        //1 as error means that is_empty is not to be trusted and that there was an
        //exception when asking the group. Binding should throw a general exception
        //InvalidOperation or the like, and in text describe that a call to is empty
        //failed in an unspecified way.
    }
}


REALM_CORE_WRAPPER_API size_t group_size( Group* group_ptr){
    try{
        return group_ptr->size();
    }
    catch (...){
        return -1;//-1 indicates an exception was thrown in core
    }
}

//should be disposed by calling unbind_table_ref
REALM_CORE_WRAPPER_API Table* group_get_or_add_table(Group* group_ptr,uint16_t* table_name,size_t table_name_len)
{   
    CSStringAccessor str(table_name,table_name_len);
    bool dummy;
    return LangBindHelper::get_or_add_table(*group_ptr,str, &dummy);
}

//langbindhelper should be extended to have get_table_by_index itself if it wsa friend with Group it could
//call the private group method that takes an index and returns a table. Round tripping via name seems a bit
//inefficient
REALM_CORE_WRAPPER_API Table* group_get_table_by_index(Group* group_ptr,size_t table_ndx)
{
    StringData sd = group_ptr->get_table_name(table_ndx);
    return LangBindHelper::get_table(*group_ptr,sd);
}

REALM_CORE_WRAPPER_API size_t group_has_table(Group* group_ptr, uint16_t * table_name,size_t table_name_len)//should be disposed by calling unbind_table_ref
{    
    CSStringAccessor str(table_name,table_name_len);
    return bool_to_size_t(group_ptr->has_table(str));
}

//return a new shared group connected to a file, no_create and durabillity level are left to the defaults defined in core
REALM_CORE_WRAPPER_API SharedGroup* new_shared_group_file_defaults(uint16_t * name,size_t name_len)
{
    CSStringAccessor str(name,name_len);
    return new SharedGroup(StringData(str));   
}

//returns NULL if an exception was thrown, otherwise a shared group handle
//exceptions are thrown usually if there is some kind of IO eror, e.g. the filename is invalid or with not enought rights
//or locked or something
REALM_CORE_WRAPPER_API SharedGroup* new_shared_group_file(uint16_t * name,size_t name_len,size_t no_create,size_t durabillity_level)
{
	try {
    CSStringAccessor str(name,name_len);
    return new SharedGroup(StringData(str),size_t_to_bool(no_create), size_t_to_durabilitylevel(durabillity_level));   
	}
	catch (...) {
		return NULL;
	}
}

REALM_CORE_WRAPPER_API void shared_group_delete(SharedGroup* g) {
    delete g;
}

//binding must ensure that the returned group is never modified
REALM_CORE_WRAPPER_API const Group* shared_group_begin_read(SharedGroup* shared_group_ptr)
{
    try {
    return &shared_group_ptr->begin_read();    
   }
    catch (...) {
    return NULL;
   }
}

//binding must ensure that the returned group is never modified
//although we return -1 on exceptions, core promises to never throw any
REALM_CORE_WRAPPER_API size_t shared_group_end_read(SharedGroup* shared_group_ptr)
{    
   try {
      shared_group_ptr->end_read();    
      return 0;
   } 
    catch (...){
    
        return -1;
    }   
}

//binding must ensure that the returned group is never modified
REALM_CORE_WRAPPER_API const Group* shared_group_begin_write(SharedGroup* shared_group_ptr)
{
   try {
      return &shared_group_ptr->begin_write();    
    }
   catch (...) {
   return NULL;
   }
}

//we cannot let exceptions flow back to C# because that only works with windows and .net
//- mono runtime crashes itself if we let an exception throw back to the c# caller
REALM_CORE_WRAPPER_API size_t shared_group_commit(SharedGroup* shared_group_ptr)
{
   try {
      shared_group_ptr->commit();
      return 0;
    } 
    catch (...)
    {
      return -1;//indicates that something went wrong. Expand with more error codes later...
   }
}


//currently, we don't transmit exception error codes back to the binding
//todo:return more specific error codes than just -1
//however, rollback() is NOEXCEPT so theretically it should never throw any errors at us
REALM_CORE_WRAPPER_API size_t shared_group_rollback(SharedGroup* shared_group_ptr)
{
    try {
      shared_group_ptr->rollback();
      return 0;//indicate success
    }
    catch(...){
        return -1;//something impossible happened
    }
}
#pragma endregion // }}}

#ifdef DYNAMIC  // clang complains when making a dylib if there is no main(). :-/
  int main() { return 0; }
#endif
    
#ifdef __cplusplus
}
#endif

