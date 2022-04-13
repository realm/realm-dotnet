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
        public static ObjectHandle GetObjectHandle(this IRealmObject iro)
        {
            return (iro.Accessor as IManagedAccessor)?.ObjectHandle;
        }

        public static Metadata GetObjectMetadata(this IRealmObject iro)
        {
            return (iro.Accessor as IManagedAccessor)?.ObjectMetadata;
        }

        //TODO Later, when we move everything to IRealmObject, this can be removed
        public static void SetManagedAccessor(this IRealmObject iro, IRealmAccessor accessor, Action copyToRealmAction = null)
        {
            ((IRealmAccessible)iro).SetManagedAccessor(accessor, copyToRealmAction);
        }

        //TODO Check if this will work even with T: IRealmObject
        public static RealmResults<T> GetBacklinksForHandle<T>(this IRealmObject iro, string propertyName, ResultsHandle resultsHandle)
            where T : RealmObjectBase
        {
            return (iro.Accessor as ManagedAccessor).GetBacklinksForHandle<T>(propertyName, resultsHandle);
        }
    }
}
