// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2021 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Xml.Xsl;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace Realms.Tests.UWP
{
    static class TestHelpersUWP
    {
        public static async Task TransformTestResults(string resultsPath)
        {
            Tests.TestHelpers.CopyBundledFileToDocuments("nunit3-junit.xslt", "nunit3-junit.xslt");
            var xsltPath = RealmConfigurationBase.GetPathToRealm("nunit3-junit.xslt");
            var xsltSf = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(xsltPath));
            var localFolderSf = ApplicationData.Current.LocalFolder;
            var xsltFileSf = await xsltSf.GetFileAsync("nunit3-junit.xslt");
            var xsltFile = await XmlDocument.LoadFromFileAsync(xsltFileSf);
            var processor = new XsltProcessor(xsltFile);
            var resultsSf = await localFolderSf.GetFileAsync(Path.GetFileName(resultsPath));
            var xmlResults = await XmlDocument.LoadFromFileAsync(resultsSf);
            var transformedXmlResults = processor.TransformToDocument(xmlResults);
            await transformedXmlResults.SaveToFileAsync(resultsSf);
        }

        public static string GetResultPath(string cmdParams)
        {
            if (!string.IsNullOrEmpty(cmdParams))
            {
                var resultStr = "--result=";
                var indexEndKey = cmdParams.IndexOf(resultStr) + resultStr.Length - 1;
                if (indexEndKey != -1)
                {
                    var indexEndValue = cmdParams.IndexOf(" ", indexEndKey);
                    if (indexEndValue != -1)
                    {
                        return cmdParams.Substring(indexEndKey + 1, indexEndValue - indexEndKey);
                    }
                }
            }
            return string.Empty;
        }
    }
}
