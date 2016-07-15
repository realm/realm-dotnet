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

#include <sstream>
#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_linklist.hpp"
#include "wrapper_exceptions.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {
  
REALM_EXPORT void linklist_add(SharedLinkViewRef* linklist_ptr, size_t row_ndx, NativeException::Marshallable& ex)
{
  handle_errors(ex, [&]() {
    (**linklist_ptr)->add(row_ndx);
  });
}

REALM_EXPORT void linklist_insert(SharedLinkViewRef* linklist_ptr, size_t link_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
  handle_errors(ex, [&]() {
    const size_t count = (**linklist_ptr)->size();
    if (link_ndx >= count)
      throw IndexOutOfRangeException("Insert into RealmList", link_ndx, count);
    (**linklist_ptr)->insert(link_ndx, row_ndx);
  });
}

REALM_EXPORT Row* linklist_get(SharedLinkViewRef* linklist_ptr, size_t link_ndx, NativeException::Marshallable& ex)
{
  return handle_errors(ex, [&]() -> Row* {
    const size_t count = (**linklist_ptr)->size();
    if (link_ndx >= count)
      throw IndexOutOfRangeException("Get from RealmList", link_ndx, count);
    auto rowExpr = (**linklist_ptr)->get(link_ndx);
    return new Row(rowExpr);
  });
}

REALM_EXPORT size_t linklist_find(SharedLinkViewRef* linklist_ptr, size_t row_ndx, size_t start_from, NativeException::Marshallable& ex)
{
  return handle_errors(ex, [&]() {
    return (**linklist_ptr)->find(row_ndx, start_from);
  });
}

REALM_EXPORT void linklist_erase(SharedLinkViewRef* linklist_ptr, size_t link_ndx, NativeException::Marshallable& ex)
{
  handle_errors(ex, [&]() {
    const size_t count = (**linklist_ptr)->size();
    if (link_ndx >= count)
      throw IndexOutOfRangeException("Erase item in RealmList", link_ndx, count);
    _impl::LinkListFriend::do_remove(***linklist_ptr,  link_ndx);
  });
}

REALM_EXPORT void linklist_clear(SharedLinkViewRef* linklist_ptr, NativeException::Marshallable& ex)
{
  handle_errors(ex, [&]() {
    (**linklist_ptr)->clear();
  });
}


REALM_EXPORT size_t linklist_size(SharedLinkViewRef* linklist_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return (**linklist_ptr)->size();
    });
}

  
REALM_EXPORT void linklist_destroy(SharedLinkViewRef* linklist_ptr)
{
    delete linklist_ptr;
}

}   // extern "C"
