using System;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Diagnostics;
using System.Globalization;

//this class contains code that handles the binding and disposing logic for C# classes that wrap tightdb classes
//a C# class will only have to implement the actual acquiring and disposing of the c++ handle, the rest of the functionality is 
//handled in here
namespace TightDbCSharp
{
    /// <summary>
    /// This class performs housekeeping reg. wrapping c++ objects.
    /// </summary>
    public abstract class Handled :IDisposable 
    {
        //following the dispose pattern discussed here http://dave-black.blogspot.dk/2011/03/how-do-you-properly-implement.html
        //a good explanation can be found here http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
        //notes reg. exceptions in constructors here http://msdn.microsoft.com/en-us/vstudio/hh184269.aspx
        /// <summary>
        /// This method can be overwritten and should free or release any c++ resources attached to this object
        /// </summary>
        protected abstract void ReleaseHandle();//overwrite this. This method will be called when c++ can free the object associated with the handle
        /// <summary>
        /// Contains the c++ pointer to a c++ object - used as a handle  when calling c++ functions
        /// </summary>
        public IntPtr Handle { get;internal set; }  //handle (in fact a pointer) to a c++ hosted Table. We must unbind this handle if we have acquired it
        private bool HandleInUse { get; set; } //defaults to false.  
        private bool HandleHasBeenUsed { get; set; } //defaults to false. If this is true, the table handle has been allocated in the lifetime of this object
        private bool NotifyCppWhenDisposing { get; set; }//if false, the table handle do not need to be disposed of, on the c++ side
        /// <summary>
        /// True if the c++ resources have been released
        /// </summary>
        public bool IsDisposed { get; private set; }


        //contains a list of handles that should be unbound
        
        private static readonly List<IntPtr> UnbindList = new List<IntPtr>();
        private static readonly List<Type> UnbindTypeList = new List<Type>();

        private void AddToUnbindList()
        {
            lock (UnbindList)//everything after this is serialized with any other calls that
                //do a lock (_unbind_list)
            {
                UnbindList.Add(Handle);
                UnbindTypeList.Add(GetType());
            }
        }

        public static int LastUnbindListSize;
        public static int HighestUnbindListSize;

        
        private void UnbindUnbindList()
        {
            if (UnbindList.Count == 0)
                //this read of Count is intentionally not locked. due to performance considerations
                //we only lock if count is not zero
                //If we get a bad/wrong value out from count
                //we might exit but that is not a problem, we'll get here again some other time
                //or we migt go on, and then inside the lock, the last=unbindList.Count will
                //read the last value correctly and potentially not loop if last was really 0
                //and we read it as something else - so also no harm done
            {
                return;
            }

            
            lock (UnbindList)
            {
                Debug.Assert(UnbindList.Count == UnbindTypeList.Count); //these lists should always be in sync
                var last = UnbindList.Count - 1;
                LastUnbindListSize = last;
                if (last > HighestUnbindListSize)
                {
                    HighestUnbindListSize = last;
                }
                while (last > -1)
                {
                    var t = UnbindTypeList[last];
                    var h = UnbindList[last];
                    
                    --last;
                    if (t == typeof (Table))
                    {
                        UnsafeNativeMethods.TableUnbind(h);
                        
                    }else 
                    if (t == typeof (Query))
                    {
                        UnsafeNativeMethods.QueryDelete(h);
                        
                    } else
                    if (t == typeof (SharedGroup))
                    {
                        UnsafeNativeMethods.QueryDelete(h);
                        
                    }else
                    if (t == typeof(TableView))
                    {
                        UnsafeNativeMethods.TableViewUnbind(h);
                        
                    }else
                    if (t == typeof(Group))
                    {
                        UnsafeNativeMethods.GroupDelete(h);                        
                    }
                }
                UnbindList.Clear();
                UnbindTypeList.Clear();
            }
        }

        /// <summary>
        /// Defaults to false. If true, this query / table / tableview / subtable / group / sharedGroup is read only and it is illegal
        /// to call any modifying function on it.
        /// Readonly objects are usually gotten either from a readonly transaction, or from a group opened from a file in readonly mode
        /// </summary>
        public bool ReadOnly { get; internal set; }

        internal Handled()
        {
        }

        //if ToUnbindList is true we should put our handle into the unbind list if we shoulve been unbound
        //if ToUnbindList is false, go on and unbind ourselves
        //ToUnbindList is true if we are getting called from the finalizer thread (if we have been GC'ed bc the
        //user was sloppy and forgot a using clause
        private void Unbind(bool toUnbindList)
        {
            if (HandleInUse)
            {
                if (NotifyCppWhenDisposing)
                {
                    //string myId = ObjectIdentification();
                  //  Console.WriteLine("Handle being released by calling cpp :{0}", myId);
                    if (toUnbindList)
                        AddToUnbindList();
                    else
                        ReleaseHandle();
                    //  Console.WriteLine("Handle released successfully :{0}", myId);
                    //Console.ReadKey();
                    //  Console.WriteLine("Continuing...");
                }
                //                else
                // {
                //      Console.WriteLine("Handle being released silently :{0}", ObjectIdentification());
                //  }
                HandleInUse = false;
            }
            else
            {
                //  If you simply create a table object and then deallocate it again without ever acquiring a table handle
                //  then no exception is raised. However, if unbind is called, and there once was a table handle,
                //  it is assumed an error situation has occoured (too many unbind calls) and an exception is raised
                if (HandleHasBeenUsed)
                {
#if DEBUGDISPOSE
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        "unbind called on {0} with no handle active anymore", ObjectIdentification()));
#endif
                }
            }
        }

       
        //store the pointer to the c++ class, and do neccessary housekeeping
        internal void SetHandle(IntPtr newHandle, bool shouldBeDisposed,bool isReadOnly)
        {            
            //Console.WriteLine("Handle being set to newhandle:{0}h shouldBeDisposed:{1} ",newHandle.ToString("X"),shouldBeDisposed);
            if (HandleInUse)
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,"SetHandle called on wrapper that already has acquired a handle :{0}",ToString()));  

            ReadOnly = isReadOnly;
            Handle = newHandle;
            HandleInUse = true;
            HandleHasBeenUsed = true;
            NotifyCppWhenDisposing = shouldBeDisposed;
  //          Console.WriteLine("Handle has been set:{0}  shouldbedisposed:{1}" , ObjectIdentification(),shouldBeDisposed);
        }

        /// <summary>
        /// Enhance toString to also show our wrapper objects in the debugger with their address in hex
        /// </summary>

        public override string ToString()
        {
            return base.ToString() + String.Format(CultureInfo.InvariantCulture,": {0:X8}", (long)Handle);//long typecast bc long can be formatted, IntPtr cannot
        }

        internal void ValidateReadWrite()
        {
            if (ReadOnly)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,"{0} {1}",ToString(), " Is Read Only and cannot be modified "));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Calling dispose will free any c++ structures created to keep track of the handled object.
        /// Dispose with no parametres is called by by the user indirectly via the using keyword, or directly by the user
        /// by him calling Displose()
        /// </summary>
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//tell finalizer it does not have to call dispose or dispose of things -we have done that already
        }

        //if called from GC  we should not dispose managed as that is unsafe, the bool tells us how we were called
        //true means we were called by the user directly or indirectly via Dispose()
        //false means we were called by the finalizer thread
        private void Dispose(bool disposeManagedToo)  //was protected virtual earlier on, can be set back to protected virtual if the need arises
        {
            if (!IsDisposed)
            {
                if (disposeManagedToo)
                {
                    //dispose of c++ stuff. We are called in the user thread so safe to dispose stuff
                    UnbindUnbindList();//First unbind any c++ handles that have been enqueued by finalizer thread
                    Unbind(false);//unbind this specific object too
                }
                else
                {
                    Unbind(true);//we are being called by the finalizer thread. It is NOT safe to call
                    //c++ as we could be running concurrently with user threads without the user having
                    //any control over this
                    //so add our handle to the UnbindList, and then lets some call from the user thread finish
                    //the c++ job later (any handled dispose call and any handled new call will try to clear the list)
                }

                //dispose any unmanaged stuff we have
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        ///when this is called by the GC, unmanaged stuff might not have been freed, and managed stuff could be in the process of being
        ///freed, so only get rid of unmanaged stuff
        /// </summary>
        ~Handled()
        {
            try
            {
                Dispose(false);
            }
// ReSharper disable RedundantEmptyFinallyBlock
            finally
            {
                // Only use this line if  starts to inherit from some other class that itself implements dispose
                //                base.Dispose();
            }
// ReSharper restore RedundantEmptyFinallyBlock
        }

    }
}
