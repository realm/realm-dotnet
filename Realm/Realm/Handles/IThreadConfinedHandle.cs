////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms
{
    /// <summary>
    /// A comon interface, implemented by all handles for thread confined objects.
    /// </summary>
    internal interface IThreadConfinedHandle
    {
        /// <summary>
        /// Creates a handle for the thread safe version of that object.
        /// </summary>
        /// <returns>A thread safe handle which can then be used to obtain a thread confined version of the object.</returns>
        ThreadSafeReferenceHandle GetThreadSafeReference();
    }
}
