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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Realms.Exceptions;

// Replaces IntPtr as a handle to a c++ realm class
// Using criticalHandle makes the binding more robust with regards to out-of-band exceptions and finalization
// We also have better guarantee that our finalizer is in fact called, especially in case of an unorderly app shutdown
// We do not use SafeHandle because our users do not call the same realm handles in a concurrent way, except if the call
// comes in via the finalizer thread
// Extra care must be taken to make sure we don't have problems with finalizer concurrency in the case that core gets finalizer concurrent
// -read up on notes reg. CriticalHandle, and SafeHandle and potential problems with concurrency that are mitigated by the C# library by
// refcounting the SafeHandle on interop calls
// I think we can save the performance penalty induced by SafeHandle because we know for sure that we will in fact never (for the same handle) call other than constructors
// concurrently - because when we are in a finalizer, the user thread for this handle is definitely dead. (except if several wrappers have been
// taken out for this particular handle - in that case we do in fact risk having several finalizing objects, and several live ones -and seen from the
// user perspective, the live ones are accessed in a serialized way)
// however, in that case (say same table taken out of a shared group 10 times) the last 9 tables are refcounted in core, so them being refcounted down
// should not affect the tenth table being accessed concurrently at the same time

// according to .net sourcecode we have a guarantee that a CriticalHandle will not get finalized while it is used in an interop call

/*
 * 3) GC.KeepAlive behavior - P/Invoke vs. finalizer thread ---- (HandleRef)
 * 4) Enforcement of the above via the type system - Don't use IntPtr anymore.
 *
 * see http://reflector.webtropy.com/default.aspx/Dotnetfx_Win7_3@5@1/Dotnetfx_Win7_3@5@1/3@5@1/DEVDIV/depot/DevDiv/releases/whidbey/NetFXspW7/ndp/clr/src/BCL/System/Runtime/InteropServices/CriticalHandle@cs/1/CriticalHandle@cs
 */

namespace Realms
{
    internal abstract class RealmHandle : SafeHandle
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
        // this again means that instead of putting handles into the unbindlist we can unbind the handles right away
        // even though we are called from a finalizer thread
        // we also use the boolean as the lock object for serializing access to the list

        // Unbind is called by criticalhandle to unbind unmanaged/native resources
        // As we don't want to call core concurrently from a finalizer thread
        // releasehandle will in fact just move ownership to the root
        // object of this object. The root object might then call unbind on this object which
        // will do the actual resource deallocation.
        // however, as far as this particular object is concerned, it has
        // removed its ownership in a controlled, unleaking way by calling root.RequestUnbind();
        // (If criticalhandle and jitter manages to pre-jit the entire call graph correctly,
        // we are calling an overridden virtual method after all (but so is private Criticalhandle cleanup ))
        // criticalhandle source here :  http://reflector.webtropy.com/default.aspx/Dotnetfx_Win7_3@5@1/Dotnetfx_Win7_3@5@1/3@5@1/DEVDIV/depot/DevDiv/releases/whidbey/NetFXspW7/ndp/clr/src/BCL/System/Runtime/InteropServices/CriticalHandle@cs/1/CriticalHandle@cs

        /// <summary>
        /// Override Unbind and put in code that actually calls core and unbinds whatever this handle is about.
        /// when this is called, it has already been verified that it is safe to call core - so just put in code that does the job.
        /// </summary>
        public abstract void Unbind();

        public virtual bool ForceRootOwnership => false;

        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// The Realm instance that owns this handle. Ownership means that this handle will be closed whenever the parent
        /// Realm is disposed.
        /// </summary>
        public readonly SharedRealmHandle? Root;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealmHandle"/> class by providing a
        /// SharedRealm parent. We should always try to pass in a parent here to ensure that the
        /// child handle gets closed as soon as the parent gets closed.
        /// </summary>
        protected RealmHandle(SharedRealmHandle? root, IntPtr handle) : base(IntPtr.Zero, true)
        {
            SetHandle(handle);

            // if we are a root object, we need a list for our children and Root is already null
            Root = root;
            root?.AddChild(this);
        }

        // called automatically but only once from criticalhandle when this handle is disposing or finalizing
        // see http://reflector.webtropy.com/default.aspx/4@0/4@0/DEVDIV_TFS/Dev10/Releases/RTMRel/ndp/clr/src/BCL/System/Runtime/InteropServices/CriticalHandle@cs/1305376/CriticalHandle@cs
        // and http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.criticalhandle.releasehandle(v=vs.110).aspx
        protected override bool ReleaseHandle()
        {
            // Invalid handles might occur if we throw in construction of one, after root is set, but before the handle has been acquired.
            // In that case, release should just do nothing at all - there is nothing to release.
            // Also, of course if we were called somehow with an invalid handle (should never happen except as stated above), it would not be a good idea to pass it to core
            if (IsInvalid)
            {
                return true;
            }

            try
            {
                // If we don't have a parent, there's not much to do but immediately unbind.
                // If the parent is closed, there's no need to ask it to unbind us - we can do
                // it immediately.
                if (Root?.IsClosed != false)
                {
                    Unbind();
                }
                else
                {
                    // as a child object we wil ask our root to unbind us (testing nomoreuserthread to determine if it can be done at once or we will just have to be put in an unbindlist)
                    // ask our root to unbind us (if it is itself finalizing) or put us into the unbind list (if it is still running)
                    // note that the this instance cannot and will never be a root itself bc root != null
                    Root.RequestUnbind(this);
                }

                return true;
            }
            catch
            {
                Debug.Fail("Failed to close native handle");

                // it would be really bad if we got an exception in here. We must not pass it on, but have to return false
                return false;
            }
        }

        public override string ToString()
        {
            return base.ToString() + handle.ToInt64().ToString("x8", CultureInfo.InvariantCulture);
        }

        protected void EnsureIsOpen()
        {
            if (Root?.IsClosed == true || IsClosed)
            {
                throw new RealmClosedException("This object belongs to a closed realm.");
            }
        }
    }
}
