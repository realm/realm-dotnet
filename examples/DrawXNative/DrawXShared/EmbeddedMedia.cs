////////////////////////////////////////////////////////////////////////////
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
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace DrawXShared
{
    // based on the SkiaSharpSample class SampleMedia
    internal static class EmbeddedMedia
    {
        private static readonly Assembly assembly;
        private static readonly string[] resources;

        static EmbeddedMedia()
        {
            assembly = typeof(EmbeddedMedia).GetTypeInfo().Assembly;
            resources = assembly.GetManifestResourceNames();
        }

        internal static Stream Load(string name)
        {
            name = $".Media.{name}";
            name = resources.FirstOrDefault(n => n.EndsWith(name));

            Stream stream = null;
            if (name != null)
            {
                stream = assembly.GetManifestResourceStream(name);
            }

            return stream;
        }

        internal static SKBitmap BitmapNamed(string filename)
        {
            using (var stream = new SKManagedStream(Load(filename)))
            {
                return SKBitmap.Decode(stream);
            }
        }
    }
}
