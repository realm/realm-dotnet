using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace RealmNet
{
    internal class SharedRealmHandle : RealmHandle
    {
        [Preserve("Constructor used by marshaling, cannot be removed by linker")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public SharedRealmHandle()
        {
        }

        protected override void Unbind()
        {
            NativeSharedRealm.destroy(handle);
        }
    }
}
