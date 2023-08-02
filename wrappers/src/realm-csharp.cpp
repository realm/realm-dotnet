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

#include <stdlib.h>
#include "realm_export_decls.hpp"

extern "C" {
REALM_EXPORT void realm_free(void* pointer)
{
        free(pointer);
}
} // extern "C"

#ifdef DYNAMIC  // clang complains when making a dylib if there is no main(). :-/
int main() { return 0; }
#endif
