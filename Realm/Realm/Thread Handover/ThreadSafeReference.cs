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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Realms.Exceptions;

namespace Realms
{
    /// <summary>
    /// An object intended to be passed between threads containing a thread-safe reference to its
    /// thread-confined object.
    /// <para/>
    /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
    /// <c>Realm.ResolveReference</c>.
    /// </summary>
    /// <remarks>
    /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
    /// <para/>
    /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
    /// Realm being pinned until the reference is deallocated.
    /// <para/>
    /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
    /// will be retained until all references have been resolved or deallocated.
    /// </remarks>
    public abstract class ThreadSafeReference
    {
        internal readonly ThreadSafeReferenceHandle Handle;

        internal readonly RealmObjectBase.Metadata Metadata;

        internal readonly Type ReferenceType;

        internal ThreadSafeReference(IThreadConfined value, Type type)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!value.IsManaged)
            {
                throw new RealmException("Cannot construct reference to unmanaged object, which can be passed across threads directly.");
            }

            if (!value.IsValid)
            {
                throw new RealmException("Cannot construct reference to invalidated object.");
            }

            Handle = value.Handle.GetThreadSafeReference();
            Metadata = value.Metadata;
            ReferenceType = type;
        }

        #region Factory

        /// <summary>
        /// Initializes a new instance of the <see cref="Query{T}"/> class.
        /// </summary>
        /// <param name="value">
        /// The thread-confined <see cref="IQueryable{T}"/> to create a thread-safe reference to. It must be a collection,
        /// obtained by calling <see cref="Realm.All"/> or a subsequent LINQ query.
        /// </param>
        /// <typeparam name="T">The type of the <see cref="RealmObject"/> or <see cref="EmbeddedObject"/> contained in the query.</typeparam>
        /// <returns>A <see cref="ThreadSafeReference"/> that can be passed to <c>Realm.ResolveReference(ThreadSafeReference.Query)</c> on a different thread.</returns>
        public static Query<T> Create<T>(IQueryable<T> value)
            where T : RealmObjectBase
        {
            return new Query<T>(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Object{T}"/> class.
        /// </summary>
        /// <param name="value">The thread-confined <see cref="RealmObject"/> or <see cref="EmbeddedObject"/> to create a thread-safe reference to.</param>
        /// <typeparam name="T">The type of the <see cref="RealmObject"/>/<see cref="EmbeddedObject"/>.</typeparam>
        /// <returns>A <see cref="ThreadSafeReference"/> that can be passed to <c>Realm.ResolveReference(ThreadSafeReference.Object)</c> on a different thread.</returns>
        public static Object<T> Create<T>(T value)
            where T : RealmObjectBase
        {
            return new Object<T>(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="List{T}"/> class.
        /// </summary>
        /// <param name="value">
        /// The thread-confined <see cref="IList{T}"/> to create a thread-safe reference to. It must be a collection
        /// that is a managed property of a <see cref="RealmObject"/> or a <see cref="EmbeddedObject"/>.
        /// </param>
        /// <typeparam name="T">The type of the objects contained in the list.</typeparam>
        /// <returns>A <see cref="ThreadSafeReference"/> that can be passed to <c>Realm.ResolveReference(ThreadSafeReference.List)</c> on a different thread.</returns>
        public static List<T> Create<T>(IList<T> value)
        {
            return new List<T>(value);
        }

        #endregion

        #region Implementations

        /// <summary>
        /// A reference to a <see cref="IQueryable{T}"/> intended to be passed between threads.
        /// <para/>
        /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
        /// <c>Realm.ResolveReference(ThreadSafeReference.Query)</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
        /// <para/>
        /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
        /// Realm being pinned until the reference is deallocated.
        /// <para/>
        /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
        /// will be retained until all references have been resolved or deallocated.
        /// </remarks>
        /// <typeparam name="T">The type of the <see cref="RealmObject"/>/<see cref="EmbeddedObject"/> contained in the query.</typeparam>
        public class Query<T> : ThreadSafeReference
            where T : RealmObjectBase
        {
            internal Query(IQueryable<T> value) : base((RealmResults<T>)value, Type.Query)
            {
            }
        }

        /// <summary>
        /// A reference to a <see cref="RealmObject"/> or an <see cref="EmbeddedObject"/> intended to be passed between threads.
        /// <para/>
        /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
        /// <c>Realm.ResolveReference(ThreadSafeReference.Object)</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
        /// <para/>
        /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
        /// Realm being pinned until the reference is deallocated.
        /// <para/>
        /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
        /// will be retained until all references have been resolved or deallocated.
        /// </remarks>
        /// <typeparam name="T">The type of the <see cref="RealmObject"/>/<see cref="EmbeddedObject"/>.</typeparam>
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "A nested class with generic argument is unlikely to be confused with System.Object.")]
        [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "This is intentional as ThreadSafeReference.Object represents an object.")]
        public class Object<T> : ThreadSafeReference
            where T : RealmObjectBase
        {
            internal Object(T value) : base(value, Type.Object)
            {
            }
        }

        /// <summary>
        /// A reference to a <see cref="IList{T}"/> intended to be passed between threads.
        /// <para/>
        /// To resolve a thread-safe reference on a target <see cref="Realm"/> on a different thread, pass it to
        /// <c>Realm.ResolveReference(ThreadSafeReference.List)</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="ThreadSafeReference"/> object must be resolved at most once.
        /// <para/>
        /// Failing to resolve a <see cref="ThreadSafeReference"/> will result in the source version of the
        /// Realm being pinned until the reference is deallocated.
        /// <para/>
        /// Prefer short-lived <see cref="ThreadSafeReference"/>s as the data for the version of the source Realm
        /// will be retained until all references have been resolved or deallocated.
        /// </remarks>
        /// <typeparam name="T">The type of the objects contained in the list.</typeparam>
        public class List<T> : ThreadSafeReference
        {
            internal List(IList<T> value) : base((RealmList<T>)value, Type.List)
            {
            }
        }

        #endregion

        internal enum Type
        {
            Object,
            List,
            Query
        }
    }
}
