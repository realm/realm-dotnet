using System;

namespace RealmNet.Interop
{
    //implements functionality such that the individual object can set up in the constructor if it should be unbound
    //or if it is safe to just use the handle without unbinding when we are finished.
    //this is relevant for instance with spec, which sometimes must be unbound, and sometimes not
    //same with group, if the group is a transaction, unbind should not be called, if it is a group, it should
    //Agressively optimized by telling GC not to put the object in the finalizer, if it is constructed with no finalization
    //demands. This will reduce the load on GC as all these handles then are not kept alive for finalization in a gc situation
    //Note that IgnoreUnbind only means don't unbind from the finalizer, C3 classes using this handle might implement dispose
    //and have that dispose called by the user or by using pattern, and do highlevel disposal, for instance this is utilized by
    //Transaction which inherits from Group, set to IgnoreUnbind (a leaked transaction will stay active until the shared group is disposed)
    //or finalized, rasising the probability that the user discovers the leak when he tries to do another transaction.

    //we could derive another class tighDbHandleNoFinalize, but currently we do not have a handle type that always should not be unbound
    internal abstract class RealmHandleOptionalUnbind : RealmHandle
    {
        internal readonly Boolean IgnoreUnbind;
                                   //if false, then the spec handle points to an internal structure in core,
                                   //that is not refcounted and should not be unbound
                                   //This is set to internal to avoid a CA warning. Probably should be protected but then the eror fires.

        protected RealmHandleOptionalUnbind()
        {
        }

        //IgnoreUnbind:
        //call with true and finalization will never be run on this handle
        //call with false will make this handle work as TightDbHandle
        //myroot:
        //call with the object responsible for serializing disposing and unbinding of all children of this instance
        //even if this instance should not be unbound, its children might, so root must be set in the child to
        //enable its children to point to root too
        internal RealmHandleOptionalUnbind(bool ignoreUnbind, RealmHandle myroot) : base(myroot)
        {
            if (ignoreUnbind)
            {//the CA 1816 on the line below can safely be ignored.
                GC.SuppressFinalize(this);//As this handle does not need to be unbound, we do not need this object to be finalized
            }
            IgnoreUnbind = ignoreUnbind;//readonly fields can be set once only.
        }


    }
}
