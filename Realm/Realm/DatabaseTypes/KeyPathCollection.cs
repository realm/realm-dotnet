﻿////////////////////////////////////////////////////////////////////////////
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

//TODO Add docs for this and the following types
public class KeyPathsCollection : IEnumerable<KeyPath>
{
    private IEnumerable<KeyPath> _collection;

    private static readonly KeyPathsCollection _shallow = new(KeyPathsCollectionType.Shallow);
    private static readonly KeyPathsCollection _default = new(KeyPathsCollectionType.Default);

    internal KeyPathsCollectionType Type { get; }

    private KeyPathsCollection(KeyPathsCollectionType type, ICollection<KeyPath>? collection = null)
    {
        Debug.Assert(type == KeyPathsCollectionType.Full == (collection?.Any() == true), "If collection isn't empty, then the type must be Full");

        Type = type;
        _collection = collection ?? Enumerable.Empty<KeyPath>();

        VerifyKeyPaths();
    }

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

    public static KeyPathsCollection Of(params KeyPath[] paths)
    {
        if (paths.Length == 0)
        {
            return new KeyPathsCollection(KeyPathsCollectionType.Shallow);
        }

        return new KeyPathsCollection(KeyPathsCollectionType.Full, paths);
    }

    public static KeyPathsCollection Shallow => _shallow;

    public static KeyPathsCollection Default => _default;

    public static implicit operator KeyPathsCollection(List<string> paths) =>
        new(KeyPathsCollectionType.Full, paths.Select(path => (KeyPath)path).ToArray());

    public static implicit operator KeyPathsCollection(List<KeyPath> paths) => new(KeyPathsCollectionType.Full, paths);

    public static implicit operator KeyPathsCollection(string[] paths) =>
        new(KeyPathsCollectionType.Full, paths.Select(path => (KeyPath)path).ToArray());

    public static implicit operator KeyPathsCollection(KeyPath[] paths) => new(KeyPathsCollectionType.Full, paths);

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

public struct KeyPath
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
}

internal enum KeyPathsCollectionType
{
    Default,
    Shallow,
    Full
}
