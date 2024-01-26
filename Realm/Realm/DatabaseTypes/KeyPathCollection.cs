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

//TODO Add docs for this and the following types
public class KeyPathsCollection : IEnumerable<KeyPath>
{
    private IEnumerable<KeyPath> _collection;

    private static readonly KeyPathsCollection _shallow = new KeyPathsCollection(KeyPathsCollectionType.Shallow);
    private static readonly KeyPathsCollection _default = new KeyPathsCollection(KeyPathsCollectionType.Default);

    internal KeyPathsCollectionType Type { get; set; }

    private KeyPathsCollection(KeyPathsCollectionType type, IEnumerable<KeyPath>? collection = null)
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
            if (string.IsNullOrWhiteSpace(item?.Path))
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
        new(KeyPathsCollectionType.Full, paths.Select(path => (KeyPath)path));

    public static implicit operator KeyPathsCollection(List<KeyPath> paths) => new(KeyPathsCollectionType.Full, paths);

    public static implicit operator KeyPathsCollection(string[] paths) =>
        new(KeyPathsCollectionType.Full, paths.Select(path => (KeyPath)path));

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

public class KeyPath
{
    internal string Path { get; private set; }

    private KeyPath(string path)
    {
        Path = path;
    }

    public static implicit operator KeyPath(string s) => new(s);
}

internal enum KeyPathsCollectionType
{
    Default,
    Shallow,
    Full
}
