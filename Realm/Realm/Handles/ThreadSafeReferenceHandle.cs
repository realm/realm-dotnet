﻿////////////////////////////////////////////////////////////////////////////
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

using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class ThreadSafeReferenceHandle : StandaloneHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "thread_safe_reference_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

#pragma warning restore IDE1006 // Naming Styles
        }

        [Preserve]
        public ThreadSafeReferenceHandle(IntPtr handle) : base(handle)
        {
        }

        protected override void Unbind() => NativeMethods.destroy(handle);
    }
}
