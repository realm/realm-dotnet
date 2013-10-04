using System;
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
        internal abstract string ObjectIdentification();//overwrite this to enable the framework to name the class in a human readable way

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
        

        internal Handled()
        {
        }

        private void Unbind()
        {
            if (HandleInUse)
            {
                if (NotifyCppWhenDisposing)
                {
                    //   Console.WriteLine("Handle being released by calling cpp :{0}", ObjectIdentification());
                    ReleaseHandle();
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
        internal void SetHandle(IntPtr newHandle, bool shouldBeDisposed)
        {            
//            Console.WriteLine("Handle being set to newhandle:{0}h shouldBeDisposed:{1} ",newHandle.ToString("X"),shouldBeDisposed);
            if (HandleInUse)
            {
                
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                                                       "SetHandle called on {0} that already has acquired a handle",
                                                       ObjectIdentification()));  
                
            }
            Handle = newHandle;
            HandleInUse = true;
            HandleHasBeenUsed = true;
            NotifyCppWhenDisposing = shouldBeDisposed;
  //          Console.WriteLine("Handle has been set:{0}  shouldbedisposed:{1}" , ObjectIdentification(),shouldBeDisposed);
        }

        //called by users who don't want to use our class anymore.
        //should free managed as well as unmanaged stuff
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//tell finalizer it does not have to call dispose or dispose of things -we have done that already
        }
        //if called from GC  we should not dispose managed as that is unsafe, the bool tells us how we were called
        private void Dispose(bool disposeManagedToo)  //was protected virtual earlier on, can be set back to protected virtual if the need arises
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
