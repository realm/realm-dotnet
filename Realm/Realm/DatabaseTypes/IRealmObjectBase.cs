////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Realms.Schema;
using Realms.Weaving;

namespace Realms
{
    /// <summary>
    /// An interface that is implemented by all objects that can be persisted in Realm.
    /// This interface is used only internally for now.
    /// </summary>
    public interface IRealmObjectBase : ISettableManagedAccessor
    {
        /// <summary>
        /// Gets the accessor that encapsulates the methods and properties used by the object for its functioning.
        /// </summary>
        IRealmAccessor Accessor { get; }

        /// <summary>
        /// Gets a value indicating whether the object has been associated with a Realm, either at creation or via
        /// <see cref="Realm.Add{T}(T, bool)"/>.
        /// </summary>
        /// <value><c>true</c> if object belongs to a Realm; <c>false</c> if standalone.</value>
        [MemberNotNullWhen(true, nameof(Realm), nameof(ObjectSchema))]
        bool IsManaged { get; }

        /// <summary>
        /// Gets a value indicating whether this object is managed and represents a row in the database.
        /// If a managed object has been removed from the Realm, it is no longer valid and accessing properties on it
        /// will throw an exception.
        /// Unmanaged objects are always considered valid.
        /// </summary>
        /// <value><c>true</c> if managed and part of the Realm or unmanaged; <c>false</c> if managed but deleted.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether this object is frozen. Frozen objects are immutable
        /// and will not update when writes are made to the Realm. Unlike live objects, frozen
        /// objects can be used across threads.
        /// </summary>
        /// <value><c>true</c> if the object is frozen and immutable; <c>false</c> otherwise.</value>
        /// <seealso cref="FrozenObjectsExtensions.Freeze{T}(T)"/>
        bool IsFrozen { get; }

        /// <summary>
        /// Gets the <see cref="Realm"/> instance this object belongs to, or <c>null</c> if it is unmanaged.
        /// </summary>
        /// <value>The <see cref="Realm"/> instance this object belongs to.</value>
        Realm? Realm { get; }

        /// <summary>
        /// Gets the <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        /// <value>A collection of properties describing the underlying schema of this object.</value>
        ObjectSchema? ObjectSchema { get; }

        /// <summary>
        /// Gets an object encompassing the dynamic API for this Realm object instance.
        /// </summary>
        /// <value>A <see cref="Dynamic"/> instance that wraps this Realm object.</value>
        public DynamicObjectApi DynamicApi { get; }

        /// <summary>
        /// Gets the number of objects referring to this one via either a to-one or to-many relationship.
        /// </summary>
        /// <value>The number of objects referring to this one.</value>
        public int BacklinksCount { get; }
    }

    /// <summary>
    /// Base interface for any object that can be persisted in a <see cref="Realm"/>.
    /// </summary>
    /// <remarks>
    /// This interface will be implemented automatically by the Realm source generator as long as your
    /// model class is declared as <c>partial</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public partial class Person : IRealmObject
    /// {
    ///     public string Name { get; set; } = "";
    /// }
    /// </code>
    /// </example>
    public interface IRealmObject : IRealmObjectBase
    {
    }

    /// <summary>
    /// Base interface for any asymmetric object that can be persisted in a <see cref="Realm"/>.
    /// </summary>
    /// <remarks>
    /// The benefit of using <see cref="IAsymmetricObject"/> is that the performance of each sync operation is much higher.
    /// The drawback is that an <see cref="IAsymmetricObject"/> is synced unidirectionally, so it cannot be queried.
    /// You should use this base when you have a write-heavy use case.
    /// If, instead you want to persist an object that you can also query against, use <see cref="IRealmObject"/> instead.
    /// <br/>
    /// This interface will be implemented automatically by the Realm source generator as long as your
    /// model class is declared as <c>partial</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public partial class SensorReading : IAsymmetricObject
    /// {
    ///     public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
    ///
    ///     public double Value { get; set; }
    /// }
    /// </code>
    /// </example>
    /// <seealso href="https://www.mongodb.com/docs/realm/sdk/dotnet/data-types/asymmetric-objects/"/>
    public interface IAsymmetricObject : IRealmObjectBase
    {
    }

    /// <summary>
    /// Base interface for any embedded object that can be persisted in a <see cref="Realm"/>.
    /// </summary>
    /// <remarks>
    /// This interface will be implemented automatically by the Realm source generator as long as your
    /// model class is declared as <c>partial</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// public partial class Address : IEmbeddedObject
    /// {
    ///     public string? City { get; set; }
    ///
    ///     public string? Country { get; set; }
    /// }
    /// </code>
    /// </example>
    public interface IEmbeddedObject : IRealmObjectBase
    {
        /// <summary>
        /// Gets the parent of the <see cref="IEmbeddedObject">embedded object</see>. It can be either another
        /// <see cref="IEmbeddedObject">embedded object</see>, a standalone <see cref="IRealmObject">realm object</see>,
        /// or an <see cref="IAsymmetricObject">asymmetric object</see>.
        /// </summary>
        /// <value>The parent object that owns this <see cref="IEmbeddedObject"/>.</value>
        public IRealmObjectBase? Parent { get; }
    }

    /// <summary>
    /// Represents an object whose managed accessor can be set.
    /// This interface is used only internally for now.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISettableManagedAccessor
    {
        /// <summary>
        /// Sets the accessor for the newly managed object and possibly adds the object to the realm.
        /// </summary>
        /// <param name="accessor">The accessor to set.</param>
        /// <param name="helper">The<see cref="IRealmObjectHelper"/> implementation to use for copying the object to realm.</param>
        /// <param name="update">If set to <c>true</c>, update the existing value (if any). Otherwise, try to add and throw if an object with the same primary key already exists.</param>
        /// <param name="skipDefaults">
        /// If set to <c>true</c> will not invoke the setters of properties that have default values.
        /// Generally, should be <c>true</c> for newly created objects and <c>false</c> when updating existing ones.
        /// </param>
        void SetManagedAccessor(IRealmAccessor accessor, IRealmObjectHelper? helper = null, bool update = false, bool skipDefaults = false);
    }
}
