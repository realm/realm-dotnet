////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Realms;

/// <summary>
/// Represents a collection of <see cref="KeyPath"/> that can be used when subscribing to notifications with <see cref="IRealmCollection{T}.SubscribeForNotifications(Realms.NotificationCallbackDelegate{T}, Realms.KeyPathsCollection?)"/>.
/// <remarks>
/// <para>
/// A <see cref="KeyPathsCollection"/> can be obtained by:
/// <list type="bullet">
///     <item>
///         <description>building it explicitly by using the method <see cref="KeyPathsCollection.Of(Realms.KeyPath[])"/>;</description>
///     </item>
///     <item>
///         <description>building it implicitly with the conversion from a <see cref="List{T}"/> or array of <see cref="KeyPath"/> or strings;</description>
///     </item>
///     <item>
///         <description>getting one of the static values <see cref="Full"/> and <see cref="Shallow"/> for full and shallow notifications respectively.</description>
///     </item>
/// </list>
/// </para>
/// </remarks>
/// </summary>
public class KeyPathsCollection : IEnumerable<KeyPath>
{
    private IEnumerable<KeyPath> _collection;

    private static readonly KeyPathsCollection _shallow = new(KeyPathsCollectionType.Shallow);
    private static readonly KeyPathsCollection _full = new(KeyPathsCollectionType.Full);

    internal KeyPathsCollectionType Type { get; }

    private KeyPathsCollection(KeyPathsCollectionType type, ICollection<KeyPath>? collection = null)
    {
        Debug.Assert(type == KeyPathsCollectionType.Explicit == (collection?.Any() == true),
            $"If collection isn't empty, then the type must be {nameof(KeyPathsCollectionType.Explicit)}");

        Type = type;
        _collection = collection ?? Enumerable.Empty<KeyPath>();

        VerifyKeyPaths();
    }

    /// <summary>
    /// Builds a <see cref="KeyPathsCollection"/> from an array of <see cref="KeyPath"/>.
    /// </summary>
    /// <param name="paths">The array of <see cref="KeyPath"/> to use for the  <see cref="KeyPathsCollection"/>.</param>
    /// <returns>The <see cref="KeyPathsCollection"/> built from the input array of <see cref="KeyPath"/>.</returns>
    public static KeyPathsCollection Of(params KeyPath[] paths)
    {
        if (paths.Length == 0)
        {
            return new KeyPathsCollection(KeyPathsCollectionType.Shallow);
        }

        return new KeyPathsCollection(KeyPathsCollectionType.Explicit, paths);
    }

    /// <summary>
    /// Gets a <see cref="KeyPathsCollection"/> value for shallow notifications, that will raise notifications only for changes to the collection itself (for example when an element is added or removed),
    /// but not for changes to any of the properties of the elements of the collection.
    /// </summary>
    public static KeyPathsCollection Shallow => _shallow;

    /// <summary>
    /// Gets a <see cref="KeyPathsCollection"/> value for full notifications, for which changes to all top-level properties and 4 nested levels will raise a notification. This is the default <see cref="KeyPathsCollection"/> value.
    /// </summary>
    public static KeyPathsCollection Full => _full;

    public static implicit operator KeyPathsCollection(List<string> paths) =>
        new(KeyPathsCollectionType.Explicit, paths.Select(path => (KeyPath)path).ToArray());

    public static implicit operator KeyPathsCollection(List<KeyPath> paths) => new(KeyPathsCollectionType.Explicit, paths);

    public static implicit operator KeyPathsCollection(string[] paths) =>
        new(KeyPathsCollectionType.Explicit, paths.Select(path => (KeyPath)path).ToArray());

    public static implicit operator KeyPathsCollection(KeyPath[] paths) => new(KeyPathsCollectionType.Explicit, paths);

    internal IEnumerable<string> GetStrings() => _collection.Select(x => x.Path);

    internal void VerifyKeyPaths()
    {
        foreach (var item in _collection)
        {
            if (string.IsNullOrWhiteSpace(item.Path))
            {
                throw new ArgumentException("A key path cannot be null, empty, or consisting only of white spaces");
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerator<KeyPath> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

/// <summary>
/// Represents a key path that can be used as a part of a <see cref="KeyPathsCollection"/> when subscribing for notifications.
/// A <see cref="KeyPath"/> can be implicitly built from a string, where the string is the name of a property (e.g "FirstName"), eventually dotted to indicated nested properties.
/// (e.g "Dog.Name"). Wildcards can also be used in key paths to capture all properties at a given level (e.g "*", "Friends.*" or "*.FirstName").
/// </summary>
public readonly struct KeyPath
{
    internal string Path { get; }

    private KeyPath(string path)
    {
        Path = path;
    }

    public static implicit operator KeyPath(string s) => new(s);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is KeyPath path && Path == path.Path;

    /// <inheritdoc/>
    public override int GetHashCode() => Path.GetHashCode();

    public static bool operator ==(KeyPath left, KeyPath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KeyPath left, KeyPath right)
    {
        return !(left == right);
    }
}

internal enum KeyPathsCollectionType
{
    Full,
    Shallow,
    Explicit
}
