/* Copyright 2016 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms
{
    internal class ResultsHandle: RealmHandle
    {
        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public ResultsHandle()
        {
        }

        protected override void Unbind()
        {
            NativeResults.destroy(handle);
        }

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

            return NativeResults.isSameInternalResults(this, (ResultsHandle) p);
        }
    }
}