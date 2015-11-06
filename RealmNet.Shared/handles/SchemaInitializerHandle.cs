﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace RealmNet
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
