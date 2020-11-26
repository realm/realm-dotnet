using Realms.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

namespace AssemblyToProcess
{
    /** N.B. Automating the verification of this functionality would be lengthy,
     * having a too little of a return for the effort put in. Hence, it was decided to test this feature manually.
     * 
     * The easiest/fastest way to do this is:
     * 1- place a breakpoint in ModuleWeaver.cs around line ~182, so to check the content of payload return from SubmitAnalytics()
     * 2- run a whatever test and check that "Sync Enabled" is true
     * 3- comment line ~26 in this file and rerun the test
     * 4- check that "Sync Enabled" is now false
     * 
     * If all went well, the functionality works as expected. However, if something isn't working as expected,
     * maybe adding a conditional breakpoint in Analytics.SearchMethodOccurrence line ~170
     * with the following condition:
     *                     type.FullName == "AssemblyToProcess.TestSyncActive"
     * should easily help you spot what's wrong.  
     */

    public class TestSyncActive
    {
        public void CreateSyncConfiguration()
        {
            var conf = new SyncConfiguration("randomText");
        }
    }
}
