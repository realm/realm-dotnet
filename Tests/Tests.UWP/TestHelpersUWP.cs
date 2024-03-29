﻿////////////////////////////////////////////////////////////////////////////
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
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Data.Xml.Xsl;
using Windows.Storage;

namespace Realms.Tests.UWP
{
    static class TestHelpersUWP
    {
        public static async Task TransformTestResults(string resultsPath)
        {
            var resultsSf = await StorageFile.GetFileFromPathAsync(resultsPath);
            var xmlResults = await XmlDocument.LoadFromFileAsync(resultsSf);

            var xsltPath = TestHelpers.CopyBundledFileToDocuments("nunit3-junit.xslt", "nunit3-junit.xslt");
            var xsltFileSf = await StorageFile.GetFileFromPathAsync(xsltPath);
            var xsltFile = await XmlDocument.LoadFromFileAsync(xsltFileSf);
            var processor = new XsltProcessor(xsltFile);

            var transformedXmlResults = processor.TransformToDocument(xmlResults);
            await transformedXmlResults.SaveToFileAsync(resultsSf);
        }
    }
}
