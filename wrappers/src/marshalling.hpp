////////////////////////////////////////////////////////////////////////////
//
// Copyright 2015 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

#ifndef MARSHALLING_HPP
#define MARSHALLING_HPP

#include <realm.hpp>
#include <realm/util/utf8.hpp>

namespace realm {
namespace binding {

class Utf16StringAccessor {
public:
    Utf16StringAccessor(uint16_t* csbuffer, size_t csbufsize)
    {
        // For efficiency, if the incoming UTF-16 string is sufficiently
        // small, we will choose an UTF-8 output buffer whose size (in
        // bytes) is simply 4 times the number of 16-bit elements in the
        // input. This is guaranteed to be enough. However, to avoid
        // excessive over allocation, this is not done for larger input
        // strings.

        error = false;
        typedef realm::util::Utf8x16<uint16_t, std::char_traits<char16_t>>Xcode;    //This might not work in old compilers (the std::char_traits<char16_t> ).     
        size_t max_project_size = 48;

        REALM_ASSERT(max_project_size <= std::numeric_limits<size_t>::max() / 4);

        size_t u8buf_size;
        if (csbufsize <= max_project_size) {
            u8buf_size = csbufsize * 4;
        }
        else {
            const uint16_t* begin = csbuffer;
            const uint16_t* end = csbuffer + csbufsize;
            u8buf_size = Xcode::find_utf8_buf_size(begin, end);
        }
        m_data.reset(new char[u8buf_size]);
        {
            const uint16_t* in_begin = csbuffer;
            const uint16_t* in_end = csbuffer + csbufsize;
            char* out_begin = m_data.get();
            char* out_end = m_data.get() + u8buf_size;
            if (!Xcode::to_utf8(in_begin, in_end, out_begin, out_end)) {
                m_size = 0;
                error = true;
                return;//calling method should handle this. We can't throw exceptions
            }
            REALM_ASSERT(in_begin == in_end);
            m_size = out_begin - m_data.get();
        }
    }

    operator realm::StringData() const //ASD has this vanished from core? REALM_NOEXCEPT
    {
        return realm::StringData(m_data.get(), m_size);
    }

    std::string to_string() const
    {
        return std::string(m_data.get(), m_size);
    }

    const char* data() const { return m_data.get();  }
    size_t size() const { return m_size;  }

    bool error;
private:
    std::unique_ptr<char[]> m_data;
    std::size_t m_size;
};

//as We've got no idea how the compiler represents an instance of DataType on the stack, perhaps it's better to send back a size_t with the value.
//we always know the size of a size_t
inline DataType size_t_to_datatype(size_t value) {
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
    return value == 1;//here i assume 1 and size_t can be compared in a meaningfull way. C# sends a size_t = 1 when true,and =0 when false
}

//send 1 for true, 0 for false.
//this function is compatible with the error checking functions in C#
//so You can send with this one, and check with an error checking one in C#
//useful if Your method has several exit paths, some of which are erorr conditions
inline size_t bool_to_size_t(bool value) {
    if (value) return 1;
    return 0;
}

//a size_t sent from C# with value 0 means durability_full, other values means durabillity_memonly, but please
//use 1 for durabillity_memonly to make room for later extensions
inline SharedGroup::DurabilityLevel size_t_to_durabilitylevel(size_t value) {
    if (value == 0)
        return SharedGroup::durability_Full;
    return SharedGroup::durability_MemOnly;
}

size_t stringdata_to_csharpstringbuffer(StringData str, uint16_t * csharpbuffer, size_t bufsize); //note bufsize is _in_16bit_words 

} // namespace binding
} // namespace realm

#endif // MARSHALLING_HPP
