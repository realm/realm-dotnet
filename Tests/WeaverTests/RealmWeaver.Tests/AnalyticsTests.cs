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
using System.Text;
using NUnit.Framework;

namespace RealmWeaver
{
    extern alias realm;

    [TestFixture]
    public class AnalyticsTests
    {
        private class AnalyticsDataStub : realm::RealmWeaver.IAnalyticsData
        {
            public string MixPanelToken => "MixPanel token";
            public byte[] UserId => new byte[] {0x01, 0x02, 0x03};
            public string AppId => "App name that should be anonymized";
            public string OsName => "Operating system";
            public string OsVersion => "OS version";
            public string TargetOs => "Target operating system";
            public string RealmVersion => "1.0.0";
        }

        [Test]
        public void ShouldAnonymizeDelicateData()
        {
            // Arrange
            var analytics = new realm::RealmWeaver.Analytics();
            var analyticsData = new AnalyticsDataStub();
            string generatedUri = null;

            // Act
            analytics.SubmitAnalytics(analyticsData, uri => generatedUri = uri.ToString());

            // Assert
            var encodedPayload = generatedUri.Substring(37, generatedUri.Length - 42);
            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload));
            Assert.That(payload, Contains.Substring("\"distinct_id\": \"03-90-58-C6-F2-C0-CB-49-2C-53-3B-0A-4D-14-EF-77-CC-0F-78-AB-CC-CE-D5-28-7D-84-A1-A2-01-1C-FB-81\""));
            Assert.That(payload, Contains.Substring("\"Anonymized MAC Address\": \"03-90-58-C6-F2-C0-CB-49-2C-53-3B-0A-4D-14-EF-77-CC-0F-78-AB-CC-CE-D5-28-7D-84-A1-A2-01-1C-FB-81\""));
            Assert.That(payload, Contains.Substring("\"Anonymized Bundle ID\": \"BD-17-7C-8B-75-16-55-59-48-79-05-46-A6-B0-51-45-D1-69-E9-B7-4B-77-8E-19-F7-22-68-FE-F0-6C-22-59\""));
        }
    }
}
