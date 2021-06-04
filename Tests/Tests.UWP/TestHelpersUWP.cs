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
            TestHelpers.CopyBundledFileToDocuments("nunit3-junit.xslt", "nunit3-junit.xslt");
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

        // TODO show nikola the funky behaviour with params from powershell and powershell core, while fine with cmd
        public static string GetResultPath(string cmdParams)
        {
            if (!string.IsNullOrEmpty(cmdParams))
            {
                var resultStr = "--result=";
                var indexStartKeyTarget = cmdParams.IndexOf(resultStr);
                if (indexStartKeyTarget != -1)
                {
                    var indexEndKeyTarget = indexStartKeyTarget + resultStr.Length - 1;
                    // if string is in apexes, don't care about spaces
                    if (cmdParams.Length > indexEndKeyTarget + 2 && cmdParams[indexEndKeyTarget + 1] == '"')
                    {
                        var indexEndValueTarget = cmdParams.IndexOf('"', indexEndKeyTarget + 2);
                        if (indexEndValueTarget != -1)
                        {
                            return cmdParams.Substring(indexEndKeyTarget + 2, indexEndValueTarget - indexEndKeyTarget - 2);
                        }
                    }
                    else
                    {
                        var indexNextParam = cmdParams.IndexOf("--", indexEndKeyTarget);
                        if (indexNextParam != -1)
                        {
                            // search the previous last char
                            var indexEndValueTarget = indexNextParam;
                            while (cmdParams[indexEndValueTarget--] == ' ') { }
                            return cmdParams.Substring(indexEndKeyTarget + 1, indexEndValueTarget - indexEndKeyTarget);
                        }
                        //if target was the last parameter
                        else
                        {
                            return cmdParams.Substring(indexEndKeyTarget + 1, cmdParams.Length - 1 - indexEndKeyTarget);
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
