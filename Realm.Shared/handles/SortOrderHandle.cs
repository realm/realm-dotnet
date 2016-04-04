/* Copyright 2016 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms
{
    internal class SortOrderHandle: RealmHandle
    {
        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a SortOrderHandle when returning a size_t as a SortOrderHandle
        [Preserve]
        public SortOrderHandle()
        {
        }

        protected override void Unbind()
        {
            NativeSortOrder.destroy(handle);
        }

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            return ReferenceEquals(this, p);
        }
    }
}