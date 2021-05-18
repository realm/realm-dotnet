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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Realms.Tests
{
    public static class TransformHelpers
    {
        public static void TransformTestResults(string resultPath, Assembly assemblyToSearchIn, string transformFilePath)
        {
            CopyBundledFileToDocuments("nunit3-junit.xslt", assemblyToSearchIn, "nunit3-junit.xslt");
            //var transformFile = transformFilePath;//RealmConfigurationBase.GetPathToRealm("nunit3-junit.xslt");

            var xpathDocument = new XPathDocument(resultPath);
            var transform = new XslCompiledTransform();
            transform.Load(transformFilePath);
            using (var writer = new XmlTextWriter(resultPath, null))
            {
                transform.Transform(xpathDocument, null, writer);
            }
        }

        public static string CopyBundledFileToDocuments(string realmName, Assembly assemblyOrigin, string destPath)
        {
            // destPath = RealmConfigurationBase.GetPathToRealm(destPath);  // any relative subdir or filename works

            //var assembly = typeof(TestHelpers).Assembly;
            var resourceName = assemblyOrigin.GetManifestResourceNames().SingleOrDefault(s => s.EndsWith(realmName));
            if (resourceName == null)
            {
                throw new Exception($"Couldn't find embedded resource '{realmName}' in the RealmTests assembly");
            }

            using (var stream = assemblyOrigin.GetManifestResourceStream(resourceName))
            using (var destination = new FileStream(destPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                stream.CopyTo(destination);
            }

            return destPath;
        }
    }
}
