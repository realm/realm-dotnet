using System;
using System.ComponentModel;
using System.Globalization;

//this class contains code that handles the binding and disposing logic for C# classes that wrap tightdb classes
//a C# class will only have to implement the actual acquiring and disposing of the c++ handle, the rest of the functionality is 
//handled in here
namespace TightDbCSharp
{
    public abstract class Handled :IDisposable 
    {
        public IntPtr Handle { get;internal set; }  //handle (in fact a pointer) to a c++ hosted Table. We must unbind this handle if we have acquired it
        internal bool HandleInUse { get; set; } //defaults to false.  TODO:this might need to be encapsulated with a lock to make it thread safe (although several threads *opening or closing* *the same* table object is probably not happening often)
        internal bool HandleHasBeenUsed { get; set; } //defaults to false. If this is true, the table handle has been allocated in the lifetime of this object
        internal bool NotifyCppWhenDisposing { get; set; }//if false, the table handle do not need to be disposed of, on the c++ side

        //use this function to set the table handle to make sure various booleans are set correctly        

        internal Handled()
        {

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal Handled(IntPtr handle,bool shouldbedisposed)
        {
            SetHandle(handle,shouldbedisposed);
        }

        internal abstract void ReleaseHandle();

        //this method sb overwritten and implement the unbinding of the handle with the c++ tighdb api
        public void Unbind()
        {
            if (HandleInUse)
            {
                if (NotifyCppWhenDisposing)
                    ReleaseHandle();
                HandleInUse = false;
            }
            else
            {
                //  If you simply create a table object and then deallocate it again without ever acquiring a table handle
                //  then no exception is raised. However, if unbind is called, and there once was a table handle,
                //  it is assumed an error situation has occoured (too many unbind calls) and an exception is raised
                if (HandleHasBeenUsed)
                {
                    throw new TableException(String.Format(CultureInfo.InvariantCulture,"unbind called on {0} with no handle active anymore",ObjectIdentification()));
                }
            }
        }

        public abstract string ObjectIdentification();


        internal void SetHandle(IntPtr newHandle, bool shouldBeDisposed)
        {
            if (HandleInUse)
            {
                throw new InvalidEnumArgumentException(String.Format(CultureInfo.InvariantCulture,
                                                       "SetHandle called on {0} that already has acquired a handle",
                                                       ObjectIdentification()));  
            }
            Handle = newHandle;
            HandleInUse = true;
            HandleHasBeenUsed = true;
            NotifyCppWhenDisposing = shouldBeDisposed;
        }

        public bool IsDisposed {  get;private  set; }
        //called by users who don't want to use our class anymore.
        //should free managed as well as unmanaged stuff
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//tell finalizer it does not have to call dispose or dispose of things -we have done that already
        }
        //if called from GC  we should not dispose managed as that is unsafe, the bool tells us how we were called
        protected virtual void Dispose(bool disposeManagedToo)
        {
            if (!IsDisposed)
            {
                if (disposeManagedToo)
                {
                    //dispose any managed members table might have
                }

                //dispose any unmanaged stuff we have
                Unbind();
                IsDisposed = true;
            }
        }

        //when this is called by the GC, unmanaged stuff might not have been freed, and managed stuff could be in the process of being
        //freed, so only get rid of unmanaged stuff
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
