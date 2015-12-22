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

#ifndef WRAPPER_EXCEPTIONS_HPP
#define WRAPPER_EXCEPTIONS_HPP

namespace realm {
  
    class IndexOutOfRangeException : public std::runtime_error
    {
      static std::string makeMessage(std::string context, size_t bad_index, size_t count)
      {
        std::ostringstream ss;
        ss << context << " index:" << bad_index << " beyond range of:" << count;
        return ss.str();
      }

    public:
      IndexOutOfRangeException(std::string message) : std::runtime_error(message) {}
      IndexOutOfRangeException(std::string context, size_t bad_index, size_t count):
        std::runtime_error(makeMessage(context, bad_index, count)) {}
    };


}   // namespace realm

#endif /* defined(WRAPPER_EXCEPTIONS_HPP) */
