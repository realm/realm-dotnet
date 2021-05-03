////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using NUnitLite;

namespace Realms.Fody.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var autorun = new AutoRun(typeof(Program).GetTypeInfo().Assembly);
            autorun.Execute(args);

            var resultPath = args.FirstOrDefault(a => a.StartsWith("--result="))?.Replace("--result=", string.Empty);
            if (!string.IsNullOrEmpty(resultPath))
            {
                TransformTestResults(resultPath);
            }

            return 0;
        }

        [SuppressMessage("Security", "CA3075:Insecure DTD processing in XML", Justification = "The xml is static and trusted.")]
        [SuppressMessage("Security", "CA5372:Use XmlReader For XPathDocument", Justification = "The xml is static and trusted.")]
        private static void TransformTestResults(string resultPath)
        {
            var transformFile = Path.Combine(Directory.GetCurrentDirectory(), "nunit3-junit.xslt");
            CopyBundledFileToDocuments("nunit3-junit.xslt", transformFile);

            var xpathDocument = new XPathDocument(resultPath);
            var transform = new XslCompiledTransform();
            transform.Load(transformFile);
            using (var writer = new XmlTextWriter(resultPath, null)) {
                transform.Transform(xpathDocument, null, writer);
            }
        }

        public static string CopyBundledFileToDocuments(string realmName, string destPath = null)
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
