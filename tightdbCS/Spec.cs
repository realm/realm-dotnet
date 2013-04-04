using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks; not portable as of 2013-4-2

//C# SPEC Class. This class is a wrapper of the C++ Spec class.
//Calls are routed this way :
//The C# spec have ordinary C# types and generally keeps the same int size across physical machine layouts
//The C# spec calls methods in TightDBCalls. These TightDBCalls generally have an ordinary C# external interface,
//and then internally call on to functions exported from the c++ DLL
//The design is so, that the C# class does not have any C++ like types or structures, except the SpecHandle variable

namespace tightdb.Tightdbcsharp
{

    //custom exception for Table class. When Table runs into a Table related error, TableException is thrown
    //some system exceptions might also be thrown, in case they have not much to do with Table operation
    //following the pattern described here http://msdn.microsoft.com/en-us/library/87cdya3t.aspx
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
        public void Dispose(bool disposemanagedtoo)
        {
            if (!IsDisposed) 
            {
                if (disposemanagedtoo) {
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
                TightDBCalls.spec_deallocate(this);
                SpecHandleInUse = false;
            }
            else
            {
                //  If you simply create a table object and then deallocate it again without ever acquiring a table handle
                //  then no exception is raised. However, if unbind is called, and there once was a table handle,
                //  it is assumed an error situation has occoured (too many unbind calls) and an exception is raised
                if (SpecHandleHasBeenUsed)
                {
                    throw new TableException("table_unbin called on a table with no table handle active anymore");
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
        internal Spec(IntPtr handle,bool notifycppwhendisposing) 
        {
            SpecHandle = handle;            
        }

        //add this field to the current spec. Will add recursively if neeeded
        public void addfield(TDBField schema)
        {            
            if (schema.type != TDB.Table)
            {
                add_column(schema.type,schema.colname);
            }
            else
            {
                TDBField[] tfa = schema.subtable.ToArray();
                Spec subspec =  add_subtable_column(schema.colname);
                subspec.addfields(tfa);
            }   
        }

        public Spec add_subtable_column(String colname)
        {
           return TightDBCalls.add_subtable_column(this,colname);
        }
        
        // will add the field list to the current spec
        public void addfields(TDBField[] fields)
        {
            foreach (TDBField field in fields) 
            {
                addfield(field);
            }
        }



        public void add_column(TDB type,String name)
        {
            TightDBCalls.spec_add_column(this, type, name);
        }

        /* try to avoid using spec in bindings
        public TDB get_column_type(long column_idx)
        {
            return TightDBCalls.spec_get_column_type(this, column_idx);
        }
        */

        //I assume column_idx is a column with a table in it, or a mixed with a table?
        public Spec get_spec(long column_idx) 
        {
            if (get_column_type(column_idx) == TDB.Table)
            {
                return TightDBCalls.spec_get_spec(this, column_idx);
            }else
            throw new SpecException("get spec(column_idx) can only be called on a subtable field");
        }

        public TDB get_column_type(long column_idx)
        {
            return TightDBCalls.spec_get_column_type(this, column_idx);
        }


        public long get_column_count()
        {
            return TightDBCalls.spec_get_column_count(this);
        }

        public string get_column_name(long column_idx)
        {
            return TightDBCalls.spec_get_column_name(this, column_idx);
        }




    }
}
