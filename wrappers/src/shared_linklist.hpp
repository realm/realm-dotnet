////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
 
#ifndef SHARED_LINKLIST_HPP
#define SHARED_LINKLIST_HPP

#include <memory>

namespace realm {
  
  /**
   Wrapper class used to hang onto the smart pointer LinkViewRef
   so we can pass a raw pointer back to C#.
   
   @see table_destroy_linklist
  */
  typedef std::shared_ptr<LinkViewRef> SharedLinkViewRef;
}

#endif  // SHARED_LINKLIST_HPP
