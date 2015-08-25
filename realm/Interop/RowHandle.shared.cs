using System;

namespace RealmNet.Interop
{
    public class RowHandle: RealmHandle, IRowHandle
    {
        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public RowHandle()
        {
        }

        protected override void Unbind()
        {
            throw new NotImplementedException();
        }

        public long RowIndex => (long)UnsafeNativeMethods.row_get_row_index(this);
        public bool IsAttached => UnsafeNativeMethods.IntPtrToBool(UnsafeNativeMethods.row_get_is_attached(this));

    }
}