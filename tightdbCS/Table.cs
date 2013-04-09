using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
//using System.Threading.Tasks; not portable as of 2013-04-02

//Tell compiler to give warnings if we publicise interfaces that are not defined in the cls standard
//http://msdn.microsoft.com/en-us/library/bhc3fa7f.aspx
[assembly: CLSCompliant(true)]

//Table class. The class represents a tightdb table.
//implements idisposable - will clean itself up (and any c++ resources it uses) when garbage collected
//If You plan to save resources, You can use it with the using syntax.



namespace Tightdb.Tightdbcsharp
{


    //custom exception for Table class. When Table runs into a Table related error, TableException is thrown
    //some system exceptions might also be thrown, in case they have not much to do with Table operation
    //following the pattern described here http://msdn.microsoft.com/en-us/library/87cdya3t.aspx
    public class TableException : Exception
    {
        public TableException()
        {
        }

        public TableException(string message)
            : base(message)
        {
        }

        public TableException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


    //TDBField is used only in the table constructor to make it easier for the user to specify any table structure without too much clutter
    //TDBField constructors of various sort, return field definitions that the table constructor then uses to figure what the table structure is
    public class TDBField
    {
        static void setinfo(TDBField t, String ColumnName, TDB FieldType)
        {
            t.colname = ColumnName;
            t.type = FieldType;
        }

        public TDBField(String ColumnName, params TDBField[] SubtablefieldsArray)
        {
            setinfo(this, ColumnName, TDB.Table);
            subtable.AddRange(SubtablefieldsArray);
        }

        public TDBField(string ColumnName, TDB ColumnType)
        {
            setinfo(this, ColumnName, ColumnType);
        }

        public TDBField(string ColumnName, String ColumnType)
        {
            if (ColumnType.ToUpper() == "INT" || ColumnType.ToUpper() == "INTEGER")
            {
                setinfo(this, ColumnName, TDB.Int);
            }
            else if (ColumnType.ToUpper() == "BOOL" || ColumnType.ToUpper() == "BOOLEAN")
            {
                setinfo(this, ColumnName, TDB.Bool);
            }
            else if (ColumnType.ToUpper() == "STRING")
            {
                setinfo(this, ColumnName, TDB.String);
            }
            else if (ColumnType.ToUpper() == "BINARY" || ColumnType.ToUpper() == "BLOB")
            {
                setinfo(this, ColumnName, TDB.Binary);
            }
            else if (ColumnType.ToUpper() == "MIXED")
            {
                setinfo(this, ColumnName, TDB.Mixed);
            }

            else if (ColumnType.ToUpper() == "DATE")
            {
                setinfo(this, ColumnName, TDB.Date);
            }

            else if (ColumnType.ToUpper() == "FLOAT")
            {
                setinfo(this, ColumnName, TDB.Float);
            }
            else if (ColumnType.ToUpper() == "DOUBLE")
            {
                setinfo(this, ColumnName, TDB.Double);
            }
            else if (ColumnType.ToUpper() == "TABLE" || ColumnType.ToUpper() == "SUBTABLE")
            {
                throw new TableException("Subtables should be specified as an array, cannot create a freestanding subtable field");
            }
            else
                throw new TableException(String.Format("Trying to initialize a tablefield with an unknown type specification Fieldname:{0}  type:{1}", ColumnName, ColumnType));
        }

        public String colname;
        public TDB type;
        private List<TDBField> subtable = new List<TDBField>();//only used if type is a subtable

        internal TDBField[] getsubtablearray()
        {
            return subtable.ToArray();
        }
    }


    namespace extentions
    {

        //todo:Add more types
        public static class myextentions
        {
            public static TDBField TDBInt(this String str)
            {
                return new TDBField(str, TDB.Int);
            }


            public static TDBField Int(this String str)
            {
                return new TDBField(str, TDB.Int);
            }

            public static TDBField Bool(this string str)
            {
                return new TDBField(str, TDB.Bool);
            }

            public static TDBField TDBBool(this string str)
            {
                return new TDBField(str, TDB.Bool);
            }

            public static TDBField TDBString(this String str)
            {
                return new TDBField(str, TDB.String);
            }

            public static TDBField String(this String str)
            {
                return new TDBField(str, TDB.String);
            }


            public static TDBField TDBBinary(this String str)
            {
                return new TDBField(str, TDB.Binary);
            }

            public static TDBField Binary(this String str)
            {
                return new TDBField(str, TDB.Binary);
            }

            public static TDBField TDBSubtable(this String str, params TDBField[] fields)
            {
                return new TDBField(str, fields);
            }

            public static TDBField Subtable(this String str, params TDBField[] fields)
            {
                return new TDBField(str, fields);
            }

            //as the TDB has a type called table, we also provide a such named constructor even though it will always be a subtable
            public static TDBField Table(this String str, params TDBField[] fields)
            {
                return new TDBField(str, fields);
            }

            public static TDBField TDBMixed(this String str)
            {
                return new TDBField(str, TDB.Mixed);
            }

            public static TDBField Mixed(this String str)
            {
                return new TDBField(str, TDB.Mixed);
            }

            public static TDBField Date(this String str)
            {
                return new TDBField(str, TDB.Date);
            }

            public static TDBField TDBDate(this String str)
            {
                return new TDBField(str, TDB.Date);
            }

            public static TDBField Float(this string str)
            {
                return new TDBField(str, TDB.Float);
            }

            public static TDBField TDBFloat(this string str)
            {
                return new TDBField(str, TDB.Float);
            }

            public static TDBField Double(this string str)
            {
                return new TDBField(str, TDB.Double);
            }

            public static TDBField TDBDouble(this string str)
            {
                return new TDBField(str, TDB.Double);
            }


        }
    }



    public class Table : IDisposable
    {
        //manual dll version info. Used when debugging to see if the right DLL is loaded, or an old one
        //the number is a date and a time (usually last time i debugged something)
        public long getdllversion_CSH()
        {
            return 1304041703;
        }


        //following the dispose pattern discussed here http://dave-black.blogspot.dk/2011/03/how-do-you-properly-implement.html
        //a good explanation can be found here http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface

        public bool IsDisposed {  get;private  set; }
        //called by users who don't want to use our class anymore.
        //should free managed as well as unmanaged stuff
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//tell finalizer it does not have to call dispose or dispose of things -we have done that already
        }
        //if called from GC  we should not dispose managed as that is unsafe, the bool tells us how we were called
        public void Dispose(bool disposemanagedtoo)
        {
            if (!IsDisposed)
            {
                if (disposemanagedtoo)
                {
                    //dispose any managed members table might have
                }

                //dispose any unmanaged stuff we have
                unbind();
                IsDisposed = true;
            }
        }

        //when this is called by the GC, unmanaged stuff might not have been freed, and managed stuff could be in the process of being
        //freed, so only get rid of unmanaged stuff
        ~Table()
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

        //always acquire a table handle
        public Table()
        {
            table_new();
        }

        //this parameter type allows the user to send a comma seperated list of TableField objects without having
        //to put them into an array first
        public Table(params TDBField[] schema)
        {
            table_new();
            Spec spec = get_spec();
            foreach (TDBField tf in schema)
            {
                spec.addfield(tf);
            }
            updatefromspec();
        }

        //allows the user to quickly create a table with a single field of a single type
        public Table(TDBField schema)
        {
            table_new();//allocate a table class in c++
            Spec spec = get_spec();//get a handle to the table's new empty spec
            spec.addfield(schema);
            updatefromspec();//build table from the spec tree structure
        }

        /*
        //allows specifying a treelike structure much like the params constructor, but this
        //version will have to have an array explicitly specified as the outermost object
        public Table(TableField1[] schema)
        {
            table_new();//allocate a table class in c++
            Spec spec = get_spec();//get a handle to the table's new empty spec
            spec.addfields(schema);
            updatefromspec();//build table from the spec tree structure
        }
        */

        /*
        //random thoughts about various accessor methods
        //Quite fast - types assumed correct when running,but Asstring needs to create an object. not good
        customers.Asstring(12,3)  = "Hans";
        customers.Asstring(12,"Firstname")  ="Hans";
        //A bit less fast - types have to be looked up in C# to determine the correct call Asstring could be a property so no object needed
        customers[12,"Firstname"].Asstring = "Hans";
        //untyped, expects object, getter and setter figures what to do. 
        customers[12,3] = "Hans";
        costomers[12,"firstname"]  ="Hans";        
        */

        public long getdllversion_CPP()
        {
            return NativeCalls.cpplibversion();
        }

        //experiments
        public object this[int RowIndex, String ColumnName]
        {
            get
            {
                switch (column_type(RowIndex))
                {
                    case TDB.Int:
                    //    return getInt(RowIndex, get_column_index(ColumnName));


                    //and add support for the rest (see TDB definition)

                    default:
                        return null;//we should probably raise an exception here
                }
            }
            set
            {
            }



        }

        //experiments
        public object this[int RowIndex, int ColumnIndex]
        {
            get
            {
                switch (column_type(RowIndex))
                {
                    case TDB.Int:
                    // return getInt(ColumnIndex, RowIndex);

                    //and add support for the rest (see TDB definition)

                    default:
                        return null;
                }
            }
            set
            {
            }
        }

        //not accessible by source not in the TightDBCSharp namespace
        //TableHandle contains the value of a C++ pointer to a C++ table
        //it is sent as a parameter to calls to the C++ DLL.

        internal IntPtr TableHandle { get; set; }  //handle (in fact a pointer) to a c++ hosted Table. We must unbind this handle if we have acquired it
        internal bool TableHandleInUse { get; set; } //defaults to false.  TODO:this might need to be encapsulated with a lock to make it thread safe (although several threads *opening or closing* *the same* table object is probably not happening often)
        internal bool TableHandleHasBeenUsed { get; set; } //defaults to false. If this is true, the table handle has been allocated in the lifetime of this object

        //This method will ask c++ to create a new table object and then the method will store the table objects handle        
        internal void table_new()
        {
            if (TableHandleInUse)
            {
                throw new TableException("table_new called on a table that already has aqcuired a table handle");
            }
            else
            {
                NativeCalls.table_new(this);
                TableHandleInUse = true;
                TableHandleHasBeenUsed = true;
            }
        }


        //This method will ask c++ to dispose of a table object created by table_new.
        //this method is for internal use only
        //it will automatically be called when the table object is disposed (or garbage collected)
        //In fact, you should not at all it on your own
        internal void unbind()
        {
            if (TableHandleInUse)
            {
                NativeCalls.table_unbind(this);
                TableHandleInUse = false;
            }
            else
            {
                //  If you simply create a table object and then deallocate it again without ever acquiring a table handle
                //  then no exception is raised. However, if unbind is called, and there once was a table handle,
                //  it is assumed an error situation has occoured (too many unbind calls) and an exception is raised
                if (TableHandleHasBeenUsed)
                {
                    throw new TableException("table_unbin called on a table with no table handle active anymore");
                }
            }
        }

        //spec getter public bc a user might want to get subtable schema on a totally empty table,and that is only available via spec atm.
        public Spec get_spec()
        {
            return NativeCalls.table_get_spec(this);
        }

        //this will update the table structure to represent whatever the earlier recieved spec has been set up to
        internal void updatefromspec()
        {
            // tightdb.Tightdbcsharp.TightDBCalls.table_update_from_spec(this);
        }

        public TDB column_type(long ColumnIndex)
        {
            return NativeCalls.table_get_column_type(this, ColumnIndex);
        }


        public long column_count()
        {
            return NativeCalls.table_get_column_count(this);
        }


        public string get_column_name(long col_idx)//unfortunately an int, bc tight might have been built using 32 bits
        {
            return NativeCalls.table_get_column_name(this, col_idx);
        }

    }

}


//various ideas for doing what is done with c++ macros reg. creation of typed tables
//An extern method that creates the table on any class tha the extern might be called on. The extern would then have to 
//use reflection to figure what fields should be stored
//would mean that You annotate all fileds that should go into the table database
// + easy to use on existing classes,  
// + easy to build a new class using well known syntax and tools
// - could easily fool user to use unsupported types, 
// - user will not have a strong typed table classs, only his own class which is of whatever type
// - User would have to annotate fields to be put in the database. Default could be no field means all goes in
// + if this was just one of many ways to create a table, it could be okay - in some cases it might be convenient

// see implementation at //EXAMPLE1


//use scenarios i can think of : 

//new program, new classes, data known at the time the code is written (like, say, a database of 1 million permutations of something, and their precalculated value)

//new program, new classes, structure known at code time, but contents not known at code time (like, user will have to import a text file where the layout is known)

//new program, new classes, structure (fields etc.) not known at runtime (could be an xml importer or something else where the scema depends on the data the program reads)

//old program, already coded classes with known data at runtime needs to be shifted from technology X  to tightdb

//old program, new classes, structure known at code time, but contents not known at code time, shifted from technology X  to tightdb

//old program, new classes, structure (fields etc.) not known at runtime , shifted from technology X to tightdb

//old program, already coded classes with known data at runtime needs to be shifted from technology X  to tightdb

//Technology X  could be : c# array, C# collection, C# stream, C# dataset
//the already coded classes could inherit from anything and could have many many properties and members, of which only a subset should be saved in tightdb

//a good tightdb binding will have support for easy transformation in all the above cases
