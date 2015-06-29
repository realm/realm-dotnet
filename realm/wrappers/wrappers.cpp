#include "wrappers.h"

#include <realm.hpp>
#include <realm/util/utf8.hpp>
#include <realm/lang_bind_helper.hpp>

#define REALM_CORE_WRAPPER_API 

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

    operator realm::StringData() const REALM_NOEXCEPT
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


  REALM_CORE_WRAPPER_API size_t realm_get_wrapper_ver()
  {
    return 20150616;
  }

  REALM_CORE_WRAPPER_API int realm_get_ver_minor()
  {
    return realm::Version::get_minor();
  }

  //return a newly constructed top level table 
  REALM_CORE_WRAPPER_API Table* new_table()//should be disposed by calling unbind_table_ref
  {
    //return reinterpret_cast<size_t>(LangBindHelper::new_table());	 
    return LangBindHelper::new_table();
  }

  

  REALM_CORE_WRAPPER_API size_t table_add_column(realm::Table* table_ptr,size_t type,  uint16_t * name,size_t name_len)
  {
    CSStringAccessor str(name,name_len);
    return table_ptr->add_column(size_t_to_datatype(type),str);
  }

  REALM_CORE_WRAPPER_API size_t table_add_empty_row(Table* table_ptr, size_t num_rows)
  {   
    return table_ptr->add_empty_row(num_rows);
  }

  //returns false=0  true=1 we use a size_t as it is likely the fastest type to return
  REALM_CORE_WRAPPER_API size_t table_get_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx)
  {    
    return bool_to_size_t(table_ptr->get_bool(column_ndx,row_ndx));
  }

  REALM_CORE_WRAPPER_API size_t table_get_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t * datatochsarp, size_t bufsize)
  {
    StringData fielddata=table_ptr->get_string(column_ndx, row_ndx);
    return stringdata_to_csharpstringbuffer(fielddata, datatochsarp,bufsize);
  }

  //call with false=0  true=1 we use a size_t as it is likely the fastest type to return
  REALM_CORE_WRAPPER_API void table_set_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx,size_t value)
  {    
    table_ptr->set_bool(column_ndx,row_ndx,size_t_to_bool(value));     
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

  //the query_bool_equal will never return null
  REALM_CORE_WRAPPER_API void query_bool_equal(Query * query_ptr, size_t columnIndex, size_t value)
  {    
    query_ptr->equal(columnIndex,size_t_to_bool(value));    
    //return &(query_ptr->equal(columnIndex,size_t_to_bool(value)));//is this okay? will I get the address of a Query object that i can call in another pinvoke?
  }

  REALM_CORE_WRAPPER_API void query_string_equal(Query * query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
  {    
    CSStringAccessor str(value, value_len);
    query_ptr->equal(columnIndex, str);
  }

  REALM_CORE_WRAPPER_API size_t query_find(Query * query_ptr, size_t begin_at_table_row) 
  {
    return query_ptr->find(begin_at_table_row);
  }

#ifdef DYNAMIC  // clang complains when making a dylib if there is no main(). :-/
  int main() { return 0; }
#endif
    
#ifdef __cplusplus
}
#endif

