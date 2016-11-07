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

using System;
using System.Runtime.InteropServices;
using Realms;

namespace Realms.Sync
{
    internal static class SharedRealmHandleExtensions
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync(Realms.Native.Configuration configuration,
                [MarshalAs(UnmanagedType.LPArray), In] Realms.Native.SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] Realms.Native.SchemaProperty[] properties,
                byte[] encryptionKey,
                Native.SyncConfiguration sync_configuration,
                out NativeException ex);
        }

        public static IntPtr OpenWithSync(this SharedRealmHandle sharedRealmHandle, Realms.Native.Configuration configuration, RealmSchema schema, byte[] encryptionKey, Native.SyncConfiguration syncConfiguration)
        {
            var marshaledSchema = new SharedRealmHandle.SchemaMarshaler(schema);

            NativeException nativeException;
            var result = NativeMethods.open_with_sync(configuration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, syncConfiguration, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }
    }
}
