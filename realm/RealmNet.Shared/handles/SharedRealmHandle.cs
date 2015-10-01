using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealmNet
{
    internal class SharedRealmHandle : RealmHandle
    {
        [Preserve("Constructor used by marshaling, cannot be removed by linker")]
        public SharedRealmHandle()
        {
        }

        protected override void Unbind()
        {
            NativeSharedRealm.delete(handle);
        }
    }
}
