/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace RealmNet
{
    internal class SchemaHandle : RealmHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public SchemaHandle(SchemaInitializerHandle schemaInitializerHandle)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                SetHandle(NativeSchema.create(schemaInitializerHandle));
            }
        }

        protected override void Unbind()
        {
            // Intentionally left blank -- the config object inside c++ has taken ownership and will 
            // delete this when necessary.
        }
    }
}
