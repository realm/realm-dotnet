using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealmNet
{
    internal class SchemaHandle : RealmHandle
    {
        [Preserve("Constructor used by marshaling, cannot be removed by linker")]
        public SchemaHandle()
        {
        }

        protected override void Unbind()
        {
            //NativeSchema.delete(handle);
        }
    }
}
