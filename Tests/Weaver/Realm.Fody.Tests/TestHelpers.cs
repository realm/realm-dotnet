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
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Realms.Fody.Tests
{
    public static class TestHelpers
    {
        public static void TransformTestResults(string resultPath)
        {
            var transformFile = Path.Combine(Directory.GetCurrentDirectory(), "nunit3-junit.xslt");
            CopyBundledFileToDocuments("nunit3-junit.xslt", transformFile);

            var xpathDocument = new XPathDocument(resultPath);
            var transform = new XslCompiledTransform();
            transform.Load(transformFile);
            using (var writer = new XmlTextWriter(resultPath, null))
            {
                transform.Transform(xpathDocument, null, writer);
            }
        }

        public static string CopyBundledFileToDocuments(string realmName, string destPath)
        {
            var assembly = typeof(Program).Assembly;
            var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(s => s.EndsWith(realmName));
            if (resourceName == null)
            {
                throw new Exception($"Couldn't find embedded resource '{realmName}' in the RealmTests assembly");
            }

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var destination = new FileStream(destPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                stream.CopyTo(destination);
            }

            return destPath;
        }
    }
}
