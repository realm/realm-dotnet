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

#include "marshalling.hpp"
#include <realm.hpp>
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"

using namespace realm;

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
size_t realm::binding::stringdata_to_csharpstringbuffer(StringData str, uint16_t * csharpbuffer, size_t bufsize) //note bufsize is _in_16bit_words 
{
    //fast check. If the buffer is very likely too small, just return immediatly with a request for a larger buffer
    if (str.size() > bufsize) {
        return str.size();
    }

    //fast check. Empty strings are handled by just returning zero, not even touching the buffer
    if (str.size() <= 0) {
        return 0;
    }
    const char* in_begin = str.data();
    const char* in_end = str.data() + str.size();

    uint16_t* out_begin = csharpbuffer;
    uint16_t* out_end = csharpbuffer + bufsize;

    typedef realm::util::Utf8x16<uint16_t, std::char_traits<char16_t>>Xcode;    //This might not work in old compilers (the std::char_traits<char16_t> ). 

    size_t size = Xcode::find_utf16_buf_size(in_begin, in_end);//Figure how much space is actually needed

    if (in_begin != in_end) {
        std::cerr << "BAD UTF8 DATA IN stringdata_tocsharpbuffer :" << str.data() << "\n";
        return -1;//bad uft8 data    
    }
    if (size > bufsize)
        return size; //bufsize is too small. Return needed size

      //the transcoded string fits in the buffer

    in_begin = str.data();
    in_end = str.data() + str.size();

    if (Xcode::to_utf16(in_begin, in_end, out_begin, out_end)) {
        size_t chars_used = out_begin - csharpbuffer;
        //csharpbuffer[chars_used-5]=0; //slightly ugly hack. C# looks for a null terminated string in the buffer, so we have to null terminate this string for C# to pick up where the end is
        return (chars_used);        //transcode complete. return the number of 16-bit characters used in the buffer,excluding the null terminator
    }
    return -1;//bad utf8 data. this cannot happen
}


