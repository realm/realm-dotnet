////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Threading.Tasks;
using Realms.Native;
using Realms.Sync;

namespace Realms
{
    internal class MongoCollectionHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_get", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get(SyncUserHandle user,
                [MarshalAs(UnmanagedType.LPWStr)] string service, IntPtr service_len,
                [MarshalAs(UnmanagedType.LPWStr)] string database, IntPtr database_len,
                [MarshalAs(UnmanagedType.LPWStr)] string collection, IntPtr collection_len,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_find_one", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_one(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                FindAndModifyOptions options,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_find", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                FindAndModifyOptions options,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_aggregate", CallingConvention = CallingConvention.Cdecl)]
            public static extern void aggregate(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string pipeline, IntPtr pipeline_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern void count(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                Int64 limit,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_insert_one", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_one(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string doc, IntPtr doc_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_insert_many", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_many(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string docs, IntPtr docs_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_delete_one", CallingConvention = CallingConvention.Cdecl)]
            public static extern void delete_one(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_delete_many", CallingConvention = CallingConvention.Cdecl)]
            public static extern void delete_many(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_update_one", CallingConvention = CallingConvention.Cdecl)]
            public static extern void update_one(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                [MarshalAs(UnmanagedType.LPWStr)] string update, IntPtr update_len,
                [MarshalAs(UnmanagedType.U1)] bool upsert,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_update_many", CallingConvention = CallingConvention.Cdecl)]
            public static extern void update_many(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                [MarshalAs(UnmanagedType.LPWStr)] string update, IntPtr update_len,
                [MarshalAs(UnmanagedType.U1)] bool upsert,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_find_one_and_update", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_one_and_update(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                [MarshalAs(UnmanagedType.LPWStr)] string update, IntPtr update_len,
                FindAndModifyOptions options,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_find_one_and_replace", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_one_and_replace(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                [MarshalAs(UnmanagedType.LPWStr)] string update, IntPtr update_len,
                FindAndModifyOptions options,
                IntPtr tcs_ptr, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_mongo_collection_find_one_and_delete", CallingConvention = CallingConvention.Cdecl)]
            public static extern void find_one_and_delete(MongoCollectionHandle handle,
                [MarshalAs(UnmanagedType.LPWStr)] string filter, IntPtr filter_len,
                FindAndModifyOptions options,
                IntPtr tcs_ptr, out NativeException ex);

#pragma warning restore SA1121 // Use built-in type alias
#pragma warning restore IDE1006 // Naming Styles
        }

        private MongoCollectionHandle(SyncUserHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public static MongoCollectionHandle Create(SyncUserHandle user, string service, string database, string collection)
        {
            var handle = NativeMethods.get(user, service, (IntPtr)service.Length, database, (IntPtr)database.Length, collection, (IntPtr)collection.Length, out var ex);
            ex.ThrowIfNecessary();

            return new MongoCollectionHandle(user, handle);
        }

        public Task<BsonPayload> FindOne(string filter, FindAndModifyOptions options)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.find_one(this, filter, filter.IntPtrLength(), options, tcs, out ex));
        }

        public Task<BsonPayload> Find(string filter, FindAndModifyOptions options)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.find(this, filter, filter.IntPtrLength(), options, tcs, out ex));
        }

        public Task<BsonPayload> Aggregate(string pipeline)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.aggregate(this, pipeline, pipeline.IntPtrLength(), tcs, out ex));
        }

        public Task<BsonPayload> Count(string filter, long? limit)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.count(this, filter, filter.IntPtrLength(), limit ?? 0, tcs, out ex));
        }

        public Task<BsonPayload> InsertOne(string doc)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.insert_one(this, doc, doc.IntPtrLength(), tcs, out ex));
        }

        public Task<BsonPayload> InsertMany(string docs)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.insert_many(this, docs, docs.IntPtrLength(), tcs, out ex));
        }

        public Task<BsonPayload> DeleteOne(string filter)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.delete_one(this, filter, filter.IntPtrLength(), tcs, out ex));
        }

        public Task<BsonPayload> DeleteMany(string filter)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.delete_many(this, filter, filter.IntPtrLength(), tcs, out ex));
        }

        public Task<BsonPayload> UpdateOne(string filter, string update, bool upsert)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.update_one(this, filter, filter.IntPtrLength(), update, update.IntPtrLength(), upsert, tcs, out ex));
        }

        public Task<BsonPayload> UpdateMany(string filter, string update, bool upsert)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) => NativeMethods.update_many(this, filter, filter.IntPtrLength(), update, update.IntPtrLength(), upsert, tcs, out ex));
        }

        public Task<BsonPayload> FindOneAndUpdate(string filter, string update, FindAndModifyOptions options)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) =>
                NativeMethods.find_one_and_update(this, filter, filter.IntPtrLength(), update, update.IntPtrLength(), options, tcs, out ex));
        }

        public Task<BsonPayload> FindOneAndReplace(string filter, string replacement, FindAndModifyOptions options)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) =>
                NativeMethods.find_one_and_replace(this, filter, filter.IntPtrLength(), replacement, replacement.IntPtrLength(), options, tcs, out ex));
        }

        public Task<BsonPayload> FindOneAndDelete(string filter, FindAndModifyOptions options)
        {
            return CallNativeMethod((IntPtr tcs, out NativeException ex) =>
                NativeMethods.find_one_and_delete(this, filter, filter.IntPtrLength(), options, tcs, out ex));
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        private delegate void NativeMethod(IntPtr tcsPtr, out NativeException ex);

        private static Task<BsonPayload> CallNativeMethod(NativeMethod method)
        {
            var tcs = new TaskCompletionSource<BsonPayload>();
            var tcsHandle = GCHandle.Alloc(tcs);
            method(GCHandle.ToIntPtr(tcsHandle), out var ex);
            ex.ThrowIfNecessary();
            return tcs.Task;
        }
    }
}