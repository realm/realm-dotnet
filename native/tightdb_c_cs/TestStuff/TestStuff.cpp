// TestStuff.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "tightdb/utf8.hpp"
#include <tightdb/lang_bind_helper.hpp>
#include <tightdb/spec.hpp>
#include <tightdb.hpp>
#include <tightdb/unique_ptr.hpp>


using namespace tightdb;


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

    TIGHTDB_ASSERT(max_project_size <= numeric_limits<size_t>::max()/4);
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









Group* new_group_file(uint16_t * name, size_t name_len)//should be disposed by calling group_delete
{  
    CSStringAccessor name2(name,name_len);
    Group* g = new Group(StringData(name2));
    return g;
}

void group_delete(Group* group_ptr )
{
    delete(group_ptr);
}



int _tmain(int argc, _TCHAR* argv[])
{///std::wstring
//     uint16_t *  test =(uint16_t * ) L"C:\\Develope\\Testgroupf";
    
 //   size_t namelen=22;
//    Group * g  = new_group_file(test,namelen);
  
    Group * g  = new Group();
  //  g->get_table("hep");
  //  std::cerr<<g->size();//use g

  //  group_delete(g);
    
    return 0;
}

