﻿using System;

namespace RealmNet
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
            NativeTable.row_delete(this);
        }

        public long RowIndex => (long)NativeTable.row_get_row_index(this);
        public bool IsAttached => NativeTable.row_get_is_attached(this)==(IntPtr)1;  // inline equiv of IntPtrToBool

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (Object.ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            return ((RowHandle) p).RowIndex == RowIndex;
        }
    }
}