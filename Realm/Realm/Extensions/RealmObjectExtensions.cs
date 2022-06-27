﻿// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;

namespace Realms.Extensions
{
    internal static class RealmObjectExtensions
    {
        public static ObjectHandle GetObjectHandle(this IRealmObjectBase iro)
        {
            return (iro.Accessor as ManagedAccessor)?.ObjectHandle;
        }

        public static Metadata GetObjectMetadata(this IRealmObjectBase iro)
        {
            return (iro.Accessor as ManagedAccessor)?.Metadata;
        }

        public static void SetManagedAccessor(this IRealmObjectBase iro, IRealmAccessor accessor, Action copyToRealmAction = null)
        {
            iro.SetManagedAccessor(accessor, copyToRealmAction);
        }

        public static RealmResults<T> GetBacklinksForHandle<T>(this IRealmObjectBase iro, string propertyName, ResultsHandle resultsHandle)
            where T : IRealmObjectBase
        {
            return (iro.Accessor as ManagedAccessor).GetBacklinksForHandle<T>(propertyName, resultsHandle);
        }
    }
}
