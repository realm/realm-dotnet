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

using System.Collections.Generic;
using System.Linq;

namespace Realms
{
    /// <summary>
    /// An object intended to be passed between threads containing a thread-safe reference to its
    /// thread-confined object.
    /// 
    /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
    /// <c>Realm.ResolveReference</c>
    /// </summary>
    /// <remarks>
    /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
    /// 
    /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
    /// Realm being pinned until the reference is deallocated.
    /// 
    /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
    //// will be retained until all references have been resolved or deallocated.
    /// </remarks>
    public abstract class ThreadSafeReference
    {
        #region Factory

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeReference.Query{T}"/> class.
        /// </summary>
        /// <param name="value">
        /// The thread-confined <see cref="IQueryable{T}"/> to create a thread-safe reference to. It must be a collection,
        /// obtained by calling <see cref="Realm.All"/> or a subsequent LINQ query.
        /// </param>
        public static Query<T> Create<T>(IQueryable<T> value) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeReference.Object{T}"/> class.
        /// </summary>
        /// <param name="value">The thread-confined <see cref="RealmObject"/> to create a thread-safe reference to.</param>
        public static Object<T> Create<T>(T value) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeReference.List{T}"/> class.
        /// </summary>
        /// <param name="value">
        /// The thread-confined <see cref="IList{T}"/> to create a thread-safe reference to. It must be a collection
        /// representing to-many relationship as a property of a <see cref="RealmObject"/>
        /// </param>
        public static List<T> Create<T>(IList<T> value) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #endregion

        #region Implementations

        /// <summary>
        /// A reference to a <see cref="IQueryable{T}"/> intended to be passed between threads.
        /// 
        /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
        /// <see cref="T:Realm.ResolveReference`1(QueryReference{T})"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
        /// 
        /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
        /// Realm being pinned until the reference is deallocated.
        /// 
        /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
        //// will be retained until all references have been resolved or deallocated.
        /// </remarks>
        public class Query<T> : ThreadSafeReference where T : RealmObject
        {
            private Query()
            {
            }
        }

        /// <summary>
        /// A reference to a <see cref="RealmObject"/> intended to be passed between threads.
        /// 
        /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
        /// <see cref="T:Realm.ResolveReference`1(ObjectReference{T})"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
        /// 
        /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
        /// Realm being pinned until the reference is deallocated.
        /// 
        /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
        //// will be retained until all references have been resolved or deallocated.
        /// </remarks>
        public class Object<T> : ThreadSafeReference where T : RealmObject
        {
            private Object()
            {
            }
        }

        /// <summary>
        /// A reference to a <see cref="IList{T}"/> intended to be passed between threads.
        /// 
        /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
        /// <see cref="T:Realm.ResolveReference`1(ListReference{T})"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
        /// 
        /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
        /// Realm being pinned until the reference is deallocated.
        /// 
        /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
        //// will be retained until all references have been resolved or deallocated.
        /// </remarks>
        public class List<T> : ThreadSafeReference where T : RealmObject
        {
            private List()
            {
            }
        }

        #endregion
    }
}
