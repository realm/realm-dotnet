using System;

namespace RealmNet
{
    public class RowHandle: RealmHandle
    {
        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public RowHandle()
        {
        }

        protected override void Unbind()
        {
            NativeRow.row_delete(handle);
        }

        public long RowIndex => (long)NativeRow.row_get_row_index(this);
        public bool IsAttached => NativeRow.row_get_is_attached(this)==(IntPtr)1;  // inline equiv of IntPtrToBool

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, p))
            {
                return true;
            }

            return ((RowHandle) p).RowIndex == RowIndex;
        }
    }
}