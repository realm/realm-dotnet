////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Realms.Tests
{
    public static class TransformHelpers
    {
        [SuppressMessage("Security", "CA3075:Insecure DTD processing in XML", Justification = "The xml is static and trusted.")]
        [SuppressMessage("Security", "CA5372:Use XmlReader For XPathDocument", Justification = "The xml is static and trusted.")]
        public static void TransformTestResults(string resultPath, string transformFilePath)
        {
            var xpathDocument = new XPathDocument(resultPath);
            var transform = new XslCompiledTransform();
            transform.Load(transformFilePath);
            using var writer = new XmlTextWriter(resultPath, null);
            transform.Transform(xpathDocument, null, writer);
        }

        public static void ExtractBundledFile(string filename, string destPath)
        {
            if (!Path.IsPathRooted(destPath))
            {
                throw new Exception($"You need to provide an absolute path, instead of {destPath}");
            }

            var assembly = typeof(TransformHelpers).Assembly;
            var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(s => s.EndsWith("." + filename));
            if (resourceName == null)
            {
                throw new Exception($"Couldn't find embedded resource '{filename}' in the RealmTests assembly");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var destination = new FileStream(destPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            stream.CopyTo(destination);
        }
    }
}
