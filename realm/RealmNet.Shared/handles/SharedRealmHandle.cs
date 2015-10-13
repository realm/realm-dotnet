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
            NativeSharedRealm.destroy(handle);
        }
    }
}
