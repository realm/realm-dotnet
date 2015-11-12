/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess
{
    class Sandbox
    {
        public string AutoStringProperty { get; set; }

        public string ManualStringProperty { get { return manualStringProperty; } set { manualStringProperty = value; } }
        private string manualStringProperty;
    }
}
