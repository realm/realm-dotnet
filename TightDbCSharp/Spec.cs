using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks; not portable as of 2013-4-2

//C# SPEC Class. This class is a wrapper of the C++ Spec class.
//Calls are routed this way :
//The C# spec have ordinary C# types and generally keeps the same int size across physical machine layouts
//The C# spec calls methods in TightDBCalls. These TightDBCalls generally have an ordinary C# external interface,
//and then internally call on to functions exported from the c++ DLL
//The design is so, that the C# class does not have any C++ like types or structures, except the SpecHandle variable

namespace TightDb.TightDbCSharp
{

    //custom exception for Table class. When Table runs into a Table related error, TableException is thrown
    //some system exceptions might also be thrown, in case they have not much to do with Table operation
    //following the pattern described here http://msdn.microsoft.com/en-us/library/87cdya3t.aspx
    [Serializable]
    public class SpecException : Exception
    {
        public SpecException()
        {
        }

        public SpecException(string message)
            : base(message)
        {
        }

        public SpecException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SpecException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }


    public class Spec : IDisposable
    {
        //not accessible by source not in te TightDBCSharp namespace
        internal IntPtr SpecHandle {get;set;}  //handle (or pointer) to a c++ hosted spec.
        internal bool SpecHandleInUse {get; set;} //defaults to false.  TODO:this might need to be encapsulated with a lock to make it thread safe (although several threads *opening or closing* *the same* table object is totally forbidden )        
        internal bool SpecHandleHasBeenUsed { get; set; } //defaults to false. If this is true, the table handle has been allocated in the lifetime of this object
        private bool IsDisposed { get; set; }
        internal bool notifycppwhendisposing{get;set;}//if false, the spechandle do not need to be disposed of, on the c++ side
        //wether to actually dispose or not is handled in tightdbcalls.cs so the spec object should act as if it should always dispose of itself
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//tell finalizer it does not have to call dispose or dispose of things -we have done that already
        }
        //if called from GC  we should not dispose managedas that is unsafe, the bool tells us how we were called
        protected virtual void Dispose(bool disposeManagedToo)
        {
            if (!IsDisposed) 
            {
                if (disposeManagedToo) {
                    //dispose any managed members table might have
                }

                //dispose any unmanaged stuff we have
                unbind();
                IsDisposed = true;
            }
        }
        
        //when tihs is called by the GC, unmanaged stuff might not have been freed, and managed stuff could be in the process of being
        //freed, so only get rid of unmanaged stuff
        
        ~Spec()
        {
            try
            {
                Dispose(false);
            }
            finally
            {
                // Only use this line if Table starts to inherit from some other class that itself implements dispose
                //                base.Dispose();
            }
        }

        

        //this method is for internal use only
        //it will automatically be called when the spec object is disposed
        //In fact, you should not at all it on your own

        internal void unbind()
        {
            if (SpecHandleInUse)
            {
                UnsafeNativeMethods.SpecDeallocate(this);
                SpecHandleInUse = false;
            }
            else
            {
                //  If you simply create a table object and then deallocate it again without ever acquiring a table handle
                //  then no exception is raised. However, if unbind is called, and there once was a table handle,
                //  it is assumed an error situation has occoured (too many unbind calls) and an exception is raised
                if (SpecHandleHasBeenUsed)
                {
                    throw new TableException("table unbind called on a table with no table handle active anymore");
                }
            }
        }
        

        //Depending on where we get the spec handle from, it could be a structure that should be
        //deleted or a structure that should not be deleted (deallocated) in c++
        //the second parameter in the constructor is indication of this spec handle should be deallocated
        //by a call to spec_delete or if c# should do nothing when the spec handle is no longer in use in c#
        //(the spec handles that need to be deleted have been allocated as new structures, the ones that
        //do not need to be deleted are pointers into structures that are owned by a table
        //This means that a spec that has been gotten from a table should not be used after that table have
        //been deallocated.
        internal Spec(IntPtr handle,bool notifycpp) 
        {
            SpecHandle = handle;
            SpecHandleInUse = true;
            SpecHandleHasBeenUsed = true;
            this.notifycppwhendisposing = notifycpp;
        }

        //add this field to the current spec. Will add recursively if neeeded
        public void AddField(Field schema)
        {
            if (schema != null)
            {
                if (schema.FieldType != DataType.Table)
                {
                    AddColumn(schema.FieldType, schema.ColumnName);
                }
                else
                {
                    Field[] tfa = schema.GetSubTableArray();
                    Spec subspec = AddSubTableColumn(schema.ColumnName);
                    subspec.AddFields(tfa);
                }
            }
            else
            {
                throw new ArgumentNullException("schema");
            }
        }

        
        // will add the field list to the current spec
        public void AddFields(Field[] fields)
        {
            if (fields != null)
            {
                foreach (Field field in fields)
                {
                    AddField(field);
                }
            }
            else
            {
                throw new ArgumentNullException("fields");
            }
        }

        public Spec AddSubTableColumn(String columnName)
        {
            return UnsafeNativeMethods.AddSubTableColumn(this, columnName);
        }

        public void AddColumn(DataType type,String name)
        {           
            UnsafeNativeMethods.SpecAddColumn(this, type, name);            
        }

        //shorthand methods as C# constants doesn't exist so we can't do AddColumn(Type_Int,"name")
        //instead we have AddIntColumn("name") which is sort of almost just as good
        public void AddIntColumn(String name)
        {
            AddColumn(DataType.Int, name);
        }

        //I assume column_idx is a column with a table in it, or a mixed with a table?
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subTable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "columnIndex")]
        public Spec GetSpec(long columnIndex) 
        {
            if (GetColumnType(columnIndex) == DataType.Table)
            {
                return UnsafeNativeMethods.SpecGetSpec(this, columnIndex);
            }else
            throw new SpecException("get spec(columnIndex) can only be called on a subTable field");
        }

        public DataType GetColumnType(long columnIndex)
        {
            return UnsafeNativeMethods.SpecGetColumnType(this, columnIndex);
        }

        

        public long ColumnCount
        {
            get { return UnsafeNativeMethods.SpecGetColumnCount(this); }            
        }
        


        public string GetColumnName(long columnIndex)
        {
            return UnsafeNativeMethods.SpecGetColumnName(this, columnIndex);
        }
    }
}
