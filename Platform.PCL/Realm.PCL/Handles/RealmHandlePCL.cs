////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

////ASD using System.Runtime.ConstrainedExecution;
////ASD using System.Security.Permissions;
////ASD using Microsoft.Win32.SafeHandles;

// Replaces IntPtr as a handle to a c++ realm class
// Using criticalHandle makes the binding more robust with regards to out-of-band exceptions and finalization
// We also have better guarantee that our finalizer is in fact called, especially in case of an unorderly app shutdown
// We do not use SafeHandle because our users do not call the same realm handles in a concurrent way, except if the call
// comes in via the finalizer thread
// Extra care must be taken to make sure we don't have problems with finalizer concurrency in the case that core gets finalizer concurrent
// -read up on notes reg. CriticalHandle, and SafeHandle and potential problems with concurrency that are mitigated by the C# library by
// refcounting the SafeHandle on interop calls
// I think we can save the performance penalty induced by SafeHandle because we know for sure that we will in fact never (for the same handle) call other than constructors
// concurrenlty - because when we are in a finalizer, the user thread for this handle is definetly dead. (except if several wrappers have been
// taken out for this particular handle - in that case we do in fact risk having several finalizing objects, and several live ones -and seen from the
// user perspective, the live ones are acessed in a serialized way)
// however, in that case (say same table taken out of a shared group 10 times) the last 9 tables are refcounted in core, so them being refcounted down
// should not affect the tenth table being accessed concurrenlty at the same time

// according to .net sourcecode we have a guarentee that a CriticalHandle will not get finalized while it is used in an interop call

/*
 * 3) GC.KeepAlive behavior - P/Invoke vs. finalizer thread ---- (HandleRef)
 * 4) Enforcement of the above via the type system - Don't use IntPtr anymore.
 *  
 * see http://reflector.webtropy.com/default.aspx/Dotnetfx_Win7_3@5@1/Dotnetfx_Win7_3@5@1/3@5@1/DEVDIV/depot/DevDiv/releases/whidbey/NetFXspW7/ndp/clr/src/BCL/System/Runtime/InteropServices/CriticalHandle@cs/1/CriticalHandle@cs
 */

namespace Realms
{
    ////ASD   [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    ////ASD    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal abstract class RealmHandle // ASD think safe to drop this base in PCL : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Every handle can potentially have an unbind list
        // If the unbind list is instantiated, this handle is a handle for a root object
        // and the unbind list will get filled with handles from finalizer threads
        // to be unbound in the user thread.

        // the unbind list is unbound in a thread safe manner whenever a new object is taken out from this handle

        // if this object is being disposed, the unbind list is emptied and nulled (this is why the lock is not on the list)

        // We can also be in an application closing scenario where user threads have been stopped
        // if we are in that scenario, a handle should not add itself to its root's unbind list, but just unbind itself
        // this is because the user thread will never run anymore anyways.
        // note that special rules apply when the app is shutting down. For instance, finalizers can run on objects still referenced by the user
        // threads (that has now been killed)

        // if NoMoreUserThread is true, then we know that no more calls into this class will come from user threads.
        // The boolean is set to true when the root object's dispose or finalizer is called.
        // The C# binding will block access to core (via IsValid) in case of dispose having been called
        // this again menas that instaed of putting handles into the unbindlist we can unbind the handles right away
        // even though we are called from a finalizer thread
        // we also use the boolean as the lock object for serializing access to the list

        // Unbind is called by criticalhandle to unbind unmanaged/native resuorces
        // As we don't want to call core concurrently from a finalizer thread
        // releasehandle will in fact just move ownership to the root
        // object of this object. The root object might then call unbind on this object which
        // will do the actual resource deallocation.
        // however, as far as this particular object is concerned, it has
        // removed its ownership in a controlled, unleaking way by calling root.ReqestUnbind();
        // (If criticalhandle and jitter manages to pre-jit the entire call graph correctly,
        // we are calling an overridden virtual method after all (but so is private Criticalhandle cleanup ))
        // criticalhandle source here :  http://reflector.webtropy.com/default.aspx/Dotnetfx_Win7_3@5@1/Dotnetfx_Win7_3@5@1/3@5@1/DEVDIV/depot/DevDiv/releases/whidbey/NetFXspW7/ndp/clr/src/BCL/System/Runtime/InteropServices/CriticalHandle@cs/1/CriticalHandle@cs

        // We cannot use CriticalHandleZeroOrMinusOneIsInvalid because it's Win32-specific. Thus, we implement it here -- it's only this one property.
        // public override bool IsInvalid {
        //     get { return handle == IntPtr.Zero || handle == new IntPtr(-1); }
        // }

        /// <summary>
        /// Override Unbind and put in code that actually calls core and unbinds whatever this handle is about.
        /// when this is called, it has already been verified that it is safe to call core - so just put in code that does the job.
        /// </summary>
        protected abstract void Unbind();

        // I am assuming that it is okay to add fields to something derived from CriticalHandle, it is mentioned in the source that it might not be, 
        // but I think that is an internal comment to msft developers

        // at creation, we must always specify the root if any, or null if the object is itself a root
        // root in this respect means the object where it and all its children must be accessed serially
        // for instance a Group from a transaction have the shared group as root, a table from such a group have
        // the shared group as root, and a subtable from the table also have the shared group as root
        // in general, you can pass on root when You are not root Yourself, otherwise pass on null
        // we expect to be in the user thread always in a constructor.
        // therefore we take the opportunity to clear root's unbindlist when we set our root to point to it
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal RealmHandle(RealmHandle root) ////ASD  : base(true)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        // please only call if unbindlist is not null
        private void LockAndUndbindList()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        // This might work in a future version of C# - when we get Generic constructors with parametres
        // Generate a TableView object with its root set to eiter parent or to parents root
        // that is, the link will be directly to the root of the collection of classes
        // a root object R will have root==null
        // all other root objects that have this root object as root, wil have root==R

        // Call like this (say we are in a TableView and want a TableView handle attached to ourself):
        //  TableViewHandle th  = RootedHandle<TableViewHAndle>(this)
        //  say we are in a Table and want a TableViewHandle as a child
        //  var th = RootedHandle<TableViewHandle>(this)
        //  or in a table and want a Query :
        //  var qr = RootedHandle<QueryHandle>(this)
        // so in general 
        //  var xx = RootedHandle<Type>(this)  //written in any RealmHandle class
        //  will create a new RealmHandle descendant of type Type where its root is set to the same
        //  root that this have (if this.root==null then it is set to this, otherwise it is set to this.root)

        // note that this handle will be constructed with no handle value. It will be invalid by default and thus, it
        // does not really matter if root is set in an atomic fashion or not - because we are in a stage before the
        // native handle value is actually set.
        // if out-of-band exceptions leaves this class constructed before it has gotten a handle to manage, it will
        // simply finalize itself silently as the finalizer in CriticalHandle will realize the 0 value of the 
        // handle and do nothing
        // legal:
        /*
            T RootedHandle<T>(RealmHandle parent) where T:RealmHandle,new()
            {
                return (parent.Root == null) ?
                    new T {Root=parent} :
                    new T {Root=parent.Root};
            }
        */

        // What i'd like:
        /*
            T RootedHandle<T>(RealmHandle parent) where T:RealmHandle, new(RealmHandle)
            {
                return (parent.Root == null) ?
                    new T(parent):
                    new T(parent.Root);
            }
        */

        private static List<RealmHandle> GetUnbindList()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        ////ASD        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        ////ASD         [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected RealmHandle() ////ASD : base(true)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        // called automatically but only once from criticalhandle when this handle is disposing or finalizing
        // see http://reflector.webtropy.com/default.aspx/4@0/4@0/DEVDIV_TFS/Dev10/Releases/RTMRel/ndp/clr/src/BCL/System/Runtime/InteropServices/CriticalHandle@cs/1305376/CriticalHandle@cs
        // and http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.criticalhandle.releasehandle(v=vs.110).aspx
        ////ASD        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual bool ReleaseHandle()  // in real imp is override from SafeHandleZeroOrMinusOneIsInvalid
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        // only call inside a lock on UnbindListLock
        private void UnbindLockedList()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        // used in the case we need to set the handle as part of a larger setup operation
        // the original SetHandle method is not callable from other classes, and we need that feature
        // so we overwrite the original one to be able to call it
        ////ASD  [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void SetHandle(IntPtr someHandle)  // in real imp is public new from SafeHandleZeroOrMinusOneIsInvalid
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public override string ToString()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        /// <summary>
        /// Called by children to this root, when they would like to 
        /// be unbound, but are (possibly) running in a finalizer thread 
        /// so it is (possibly) not safe to unbind then directly.
        /// </summary>
        /// <param name="handleToUnbind">The core handle that is not needed anymore and should be unbound.</param>
        private void RequestUnbind(RealmHandle handleToUnbind)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}