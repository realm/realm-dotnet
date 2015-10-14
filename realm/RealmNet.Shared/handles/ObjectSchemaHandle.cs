using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace RealmNet
{
    internal class ObjectSchemaHandle : RealmHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public ObjectSchemaHandle(string name)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                SetHandle(NativeObjectSchema.create(name));
            }
        }

        protected override void Unbind()
        {
            NativeObjectSchema.destroy(handle);
        }
    }
}
