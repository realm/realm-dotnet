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

#ifndef REALM_UTF16_ACCESSOR_HPP
#define REALM_UTF16_ACCESSOR_HPP

#include <realm.hpp>
#include <realm/util/utf8.hpp>

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
    bool error;
private:
    std::unique_ptr<char[]> m_data;
    std::size_t m_size;
};

#endif /* defined(REALM_UTF16_ACCESSOR_HPP) */
