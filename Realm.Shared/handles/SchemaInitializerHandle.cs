/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace Realms
{
    internal class SchemaInitializerHandle : RealmHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public SchemaInitializerHandle()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                SetHandle(NativeSchema.initializer_create());
            }
        }

        protected override void Unbind()
        {
            NativeSchema.initializer_destroy(handle);
        }
    }
}
