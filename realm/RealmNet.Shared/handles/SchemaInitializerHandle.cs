using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealmNet
{
    internal class SchemaInitializerHandle : RealmHandle
    {
        [Preserve("Constructor used by marshaling, cannot be removed by linker")]
        public SchemaInitializerHandle()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try { /**/ }
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
