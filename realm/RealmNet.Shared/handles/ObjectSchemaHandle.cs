using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealmNet
{
    internal class ObjectSchemaHandle : RealmHandle
    {
        public ObjectSchemaHandle(string name)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try { /**/ }
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
