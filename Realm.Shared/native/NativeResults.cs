﻿////////////////////////////////////////////////////////////////////////////
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

namespace Realms
{
    internal static class NativeResults
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_is_same_internal_results",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr is_same_internal_results(ResultsHandle lhs, ResultsHandle rhs);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_create_for_table", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_table(SharedRealmHandle sharedRealm, TableHandle handle, IntPtr objectSchema);
        
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_create_for_table_sorted", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_table_sorted(SharedRealmHandle sharedRealm, TableHandle handle, IntPtr objectSchema, SortOrderHandle sortOrderHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_create_for_query", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_query(SharedRealmHandle sharedRealm, QueryHandle queryPtr, IntPtr objectSchema);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_create_for_query_sorted", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_query_sorted(SharedRealmHandle sharedRealm, QueryHandle queryPtr, IntPtr objectSchema, SortOrderHandle sortOrderHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy(IntPtr resultsHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_row", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_row(ResultsHandle results, IntPtr index);

        [DllImport (InteropConfig.DLL_NAME, EntryPoint = "results_count", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr count(ResultsHandle results);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void clear(ResultsHandle results);

        [StructLayout(LayoutKind.Sequential)]
        internal struct CollectionChangeSet
        {
            public MarshalledVector<IntPtr> Deletions;
            public MarshalledVector<IntPtr> Insertions;
            public MarshalledVector<IntPtr> Modifications;

            [StructLayout(LayoutKind.Sequential)]
            public struct Move
            {
                public IntPtr From;
                public IntPtr To;
            }
            public MarshalledVector<Move> Moves;
        }

        internal delegate void NotificationCallback(IntPtr managedResultsHandle, PtrTo<CollectionChangeSet> collectionChanges, PtrTo<NativeException> notficiationException);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr add_notification_callback(ResultsHandle results, IntPtr managedResultsHandle, NotificationCallback callback);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy_notificationtoken", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr destroy_notificationtoken(IntPtr token);
    }
}
