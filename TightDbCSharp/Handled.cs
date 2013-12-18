using System;
using System.Globalization;

//this class contains code that handles the binding and disposing logic for C# classes that wrap tightdb classes
//We use descendants of CriticalHandle to store the c++ pointer or reference.
//The Handled class contains generic wrapper stuff that is common for all C# tightdb classes

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
        //please also read the lengthy discusson on dispose, wrappers, handles etc. in ConcurrencyNotes.txt
        //A lot of the lowlevel work is now done in the TightDbHandle class and its descendants - these classes are used as handles, and 
        //guarentees for instance that the finalizer will not finalize a TableHandle before all ordinary objects have been finalized.
        //The Handled class is simply a collection of general stuff that glues the C# classes we expose users to, with the C# classes that
        //are responsible for handle management
        //the handled class itself does not have a finalizer, and its main objective is to call unbind when it is being disposed in the user thread
        //as handled has no finalizer, the TightDbHandle classes have gotten enough info for them to finalize c++ stuff themselves.
        //this ensures faster garbage collection of classes derived from Handled - they do not have to go to the finalizer queue


        //private bool HandleInUse { get; set; } //defaults to false.
        //private bool HandleHasBeenUsed { get; set; } //defaults to false. If this is true, the table handle has been allocated in the lifetime of this object


        /// <summary>
        /// True if the c++ resources have been released
        /// True if dispose have been called one way or the other
        /// </summary>
        public bool IsDisposed
        {
            get { return _handle!=null && _handle.IsClosed; }
        }




        /// <summary>
        /// Defaults to false. If true, this query / table / tableview / subtable / group / sharedGroup is read only and it is illegal
        /// to call any modifying function on it.
        /// Readonly objects are usually gotten either from a readonly transaction, or from a group opened from a file in readonly mode
        /// </summary>
        public bool ReadOnly { get; internal set; }

        protected TightDbHandle Handle
        {
            get { return _handle; }            
        }

        private TightDbHandle _handle;//will store the c++ handle for this class. The actual type is specific for what is wrapped,
        //protected because we want other classes to use the specific handle, for instance TableView.TableViewHandle instead of TableView.Handle
        //e.g. a SharedGroup will have a SharedGroupHandle stored here, and SharedGroup will have a SharedGroupHandle property that returns this hande
        //as SharedGroupHandle (because as is faster than a typecast)

        internal Handled()
        {
        }


       
        //store the pointer to the c++ class, and do neccessary housekeeping
        //now, shouldbedisposed should already have been set atomically inside the newHandle class
        internal void SetHandle(TightDbHandle newHandle,bool isReadOnly)
        {            
            ReadOnly = isReadOnly;
            _handle = newHandle;
        }

        /// <summary>
        /// Enhance toString to also show our wrapper objects in the debugger with their address in hex
        /// </summary>

        public override string ToString()
        {
            return base.ToString()+" Handle:" + _handle;//calls Handle.ToString()
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
            GC.SuppressFinalize(this);//tell finalizer thread/GC it does not have to call finalize - we have already disposed of unmanaged resources
            //above is added in case someone inherits from a handled resource and implements a finalizer - the binding does not need the call, as the
            //binding does not introduce finalizers in any C# wrapper classes (only in the Handle classes via the finalizer in CriticalHandle
            //if we decide to make the user facing tightdb classes (table,tableview,group, sharedgroup etc. final, then we can save above call)
            //todo:Measure by test and by code inspection any performance gains from not calling SuppressFinalize(this) and making the classes final
        }

        //using a very simple dispose pattern as we will just call on to Handle.Dispose in both a finalizing and in a disposing situation
        //leaving this method in here so that classes derived from this one can implement a finalizer and have that finalizer call dispose(false)
        /// <summary>
        /// Override this if you have managed stuff that needs to be closed down when dispose is called
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)  //was protected virtual earlier on, can be set back to protected virtual if the need arises
        {
            if (_handle!=null && !IsDisposed)//handle could be null if we crashed in the constructor (group with filename to a OS protected area for instance)
            {
                //no matter if we are being called from a dispose in a user thread, or from a finalizer, we should
                //ask Handle to dispose of itself (unbind)
                _handle.Dispose();
            }
        }
    }
}
