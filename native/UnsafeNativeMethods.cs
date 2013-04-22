using System;
using System.Text;
using System.Runtime.InteropServices;

//using System.Appdomain;


namespace TightDbCSharp
{
    using System.IO;
    using System.Globalization;
    using System.Reflection;
    
    //mirrors the enum in the C interface
    /*     
   Note: These must be kept in sync with those in
   <tightdb/data_type.hpp> of the core library. 
enum DataType {
    type_Int    =  0,
    type_Bool   =  1,
    type_Date   =  7,
    type_Float  =  9,
    type_Double = 10,
    type_String =  2,
    type_Binary =  4,
    type_Table  =  5,
    type_Mixed  =  6
};
     */

    public enum DataType
    {
        Int        =  0,
        Bool       =  1,
        String     =  2,
        Binary     =  4,
        Table      =  5,
        Mixed      =  6,
        Date       =  7,
        Float      =  9,
       Double      = 10
    }


    
    
    

    //this class contains methods for calling the c++ TightDB system, which has been flattened out as C type calls   
    //The individual public methods call the C inteface using types and values suitable for the C interface, but the methods take and give
    //values that are suitable for C# (for instance taking a C# Table object parameter and calling on with the C++ Table pointer inside the C# Table Class)
    //it is assumed that the deployment has copied the correct c++ dll into the same dir as where the calling assembly resides.
    //If this is not the case, a "badimage" exception runtime error will happen
    //we might want to catch that exception, then copy the correct dll into place, and then see if we can resume operation
    //alternatively even proactively copying the dll at startup if we detect that the wrong one is there
    //could be as simple as comparing file sizes or the like
    //because we expect the end user to have deployed the correct c++ dll this assembly is AnyCpu
    static class UnsafeNativeMethods
    {
        static bool Is64Bit 
        {
            get 
            {
                return (UIntPtr.Size == 8);//if this is evaluated every time, a faster way could be implemented. Size is cost when we are running though so perhaps it gets inlined by the JITter
            } 
        }

        static StringBuilder _callog;//used to collect call data when debugging small unit tests

        static bool _loggingEnabled;
        static bool LoggingEnabled
        {
            get
            {
                return _loggingEnabled;
            }
            set
            {
                if (value)
                {
                    if (_callog == null) { _callog = new StringBuilder(); }
                    _callog.AppendLine("--------LOGGING HAS BEEN ENABLED----------");
                    _loggingEnabled = true;
                }
                else
                {
                    _callog.AppendLine("--------LOGGING HAS BEEN DISABLED----------");
                    _loggingEnabled = false;
                }
            }
        }

        static public void LoggingSaveFile(String fileName)
        {
            if (LoggingEnabled)
            {
                File.WriteAllText(fileName,_callog.ToString());
            }
            
        }

        static public void LoggingDisable()
        {
            LoggingEnabled = false;
        }

        static public void LoggingEnable(string marker)
        {
            LoggingEnabled = true;
            if (!String.IsNullOrEmpty(marker))
            {
                _callog.AppendLine(String.Format(CultureInfo.InvariantCulture,"LOGGING ENABLED BY:{0}",marker));
            }
        }
        

        public static void Log(string where,string desc, params  object[] values)
        {
            if (LoggingEnabled)
            {
                _callog.Append(String.Format(CultureInfo.InvariantCulture,  "{0:yyyy-MM-dd HH:mm:ss} {1} {2}",DateTime.UtcNow, where,desc));
                _callog.AppendLine(":");
                foreach (object o in values)
                {
                    string typestr = o.GetType().ToString();
                    string valuestr = o.ToString();
                    //if something doesn't auto.generate into readable code, we can test on o, and create custom more readable values
                    Type oType = o.GetType();
                    if (oType==typeof(Table))
                    {
                        var table = o as Table;
                        if (table != null) valuestr = table.TableHandle.ToString();
                    }

                    _callog.Append("Type(");
                    _callog.Append(typestr);
                    _callog.Append(") Value(");
                    _callog.Append(valuestr);
                    _callog.AppendLine(")");
                }
            }
        }

        //tightdb_c_cs_API size_t tightdb_c_csGetVersion(void)
        [DllImport("tightdb_c_cs64",EntryPoint="tightdb_c_cs_GetVer" , CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr tightdb_c_cs_DllVer64();

        [DllImport("tightdb_c_cs32", EntryPoint = "tightdb_c_cs_GetVer", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr tightdb_c_cs_DllVer32();

        public static long CppDllVersion()
        {
            if (Is64Bit)
                return (long) tightdb_c_cs_DllVer64();
            else
                return (long) tightdb_c_cs_DllVer32();
        }


        [DllImport("tightdb_c_cs64", EntryPoint="spec_deallocate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate64(IntPtr spec);
        [DllImport("tightdb_c_cs32", EntryPoint = "spec_deallocate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate32(IntPtr spec);

        public static void SpecDeallocate(Spec s)
        {
            if (s.Notifycppwhendisposing)//some spec's we get from C++ should not be deallocated by us
            {
                if (Is64Bit) 
                  spec_deallocate64(s.SpecHandle);
                else
                  spec_deallocate32(s.SpecHandle);
                s.SpecHandle = IntPtr.Zero;
            }
        }




// tightdb_c_cs_API size_t add_column(size_t SpecPtr,DataType type, const char* name) 

        //marshalling : not sure the simple enum members have the same size on C# and c++ on all platforms and bit sizes
        //and not sure if the marshaller will fix it for us if they are not of the same size
        //so this must be tested on various platforms and bit sizes, and perhaps specific versions of calls with enums have to be made
        //this one works on windows 7, .net 4.5 32 bit, tightdb 32 bit (on a 64 bit OS, but that shouldn't make a difference)
        [DllImport("tightdb_c_cs32", EntryPoint="spec_add_column", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        
        private static extern UIntPtr spec_add_column32(IntPtr spechandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);

        
        
        
        
        
        
        [DllImport("tightdb_c_cs64", EntryPoint = "spec_add_column", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern UIntPtr spec_add_column64(IntPtr spechandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);
      
        public static long SpecAddColumn(Spec spec, DataType type, string name)
        {                   
            if(Is64Bit)
               return (long) spec_add_column64(spec.SpecHandle, type, name);
            else
                return (long)spec_add_column32(spec.SpecHandle, type, name);
        }


        [DllImport("tightdb_c_cs64", EntryPoint="table_add_column", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern UIntPtr table_add_column64(IntPtr tableHandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);
        [DllImport("tightdb_c_cs32",EntryPoint="table_add_column", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern UIntPtr table_add_column32(IntPtr tableHandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);

        public static long TableAddColumn(Table table, DataType type, string name)
        {
            if (Is64Bit)
                return (long)table_add_column64(table.TableHandle, type, name);//BM told me that column number sb long always in C#            
            else
                return (long)table_add_column32(table.TableHandle, type, name);//BM told me that column number sb long always in C#            
        }


        [DllImport("tightdb_c_cs64",EntryPoint="spec_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType spec_get_column_type64(IntPtr spechandle, IntPtr columnIndex);
        [DllImport("tightdb_c_cs32", EntryPoint="spec_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType spec_get_column_type32(IntPtr spechandle, IntPtr columnIndex);

        public static DataType SpecGetColumnType(Spec s, long columnIndex)
        {
            if (Is64Bit)
                return spec_get_column_type64(s.SpecHandle, (IntPtr)columnIndex);//the IntPtr cast of a long works on 32bit .net 4.5
            else
                return spec_get_column_type32(s.SpecHandle, (IntPtr)columnIndex);//the IntPtr cast of a long works on 32bit .net 4.5
        }

        /* not really needed
        public static DataType spec_get_column_type(Spec s, int columnIndex)
        {
            return spec_get_column_type(s.SpecHandle, (IntPtr)columnIndex);//the IntPtr cast of an int works on 32bit .net 4.5
        }                                                                   //but probably throws an exception or warning on 64bit 
        */
        //Spec add_subtable_column(const char* name);        
        [DllImport("tightdb_c_cs64", EntryPoint= "spec_add_subtable_column", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_add_subtable_column64(IntPtr spec,[MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport("tightdb_c_cs32", EntryPoint= "spec_add_subtable_column",CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_add_subtable_column32(IntPtr spec, [MarshalAs(UnmanagedType.LPStr)] string name);


        public static Spec AddSubTableColumn(Spec spec,String name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name","Adding a sub table column with 'name' set to null is not allowed");
            }
            IntPtr specHandle = Is64Bit ? spec_add_subtable_column64(spec.SpecHandle, name) : spec_add_subtable_column32(spec.SpecHandle, name);

            return new Spec(specHandle,true);//because this spechandle we get here should be deallocated
        }

        //get a spec given a column index. Returns specs for subtables, but not for mixed (as they would need a row index too)
        //Spec       *spec_get_spec(Spec *spec, size_t column_ndx);
        [DllImport("tightdb_c_cs64", EntryPoint="spec_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec64(IntPtr spec, IntPtr columnIndex);
        [DllImport("tightdb_c_cs32", EntryPoint = "spec_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec32(IntPtr spec, IntPtr columnIndex);


        private const string ErrColumnNotTable = "SpecGetSpec called with a column index for a column that is not a table";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SpecGetSpec")]
        public static Spec SpecGetSpec(Spec spec, long columnIndex)
        {
            if (spec.GetColumnType(columnIndex) != DataType.Table)
            {
                throw new SpecException(ErrColumnNotTable);
            }            
            if (Is64Bit)
                return new Spec (spec_get_spec64(spec.SpecHandle, (IntPtr)columnIndex),true);                
            else
                return new Spec(spec_get_spec32(spec.SpecHandle, (IntPtr)columnIndex), true);            
        }

        /*not really needed
        public static Spec spec_get_spec(Spec spec, int ColumnIndex)
        {
            if (spec.get_column_type(ColumnIndex) == DataType.Table)
            {
                IntPtr SpecHandle = spec_get_spec((IntPtr)spec.SpecHandle, (IntPtr)ColumnIndex);
                return new Spec(SpecHandle, true);
            }
            else
                throw new SpecException(err_column_not_table);
        }*/


        //size_t spec_get_column_count(Spec* spec);
        [DllImport("tightdb_c_cs64", EntryPoint="spec_get_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_count64(IntPtr spec);
        [DllImport("tightdb_c_cs32", EntryPoint="spec_get_column_count",CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_count32(IntPtr spec);


        public static long SpecGetColumnCount(Spec spec)
        {
            if (Is64Bit)
              return (long) spec_get_column_count64(spec.SpecHandle);//OPTIMIZE  - could be optimized with a 32 or 64 bit specific implementation - see end of file
            else
              return (long)spec_get_column_count32(spec.SpecHandle);//OPTIMIZE  - could be optimized with a 32 or 64 bit specific implementation - see end of file
        }


        //tightdb_c_cs_API size_t new_table()
        [DllImport("tightdb_c_cs64",EntryPoint="new_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr new_table64();
        [DllImport("tightdb_c_cs32", EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr new_table32();


        public static void TableNew(Table table)
        {
            table.SetTableHandle(Is64Bit ? new_table64() : new_table32(), true);
        }


        //tightdb_c_cs_API size_t new_table()
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable64(IntPtr tableHandle, IntPtr columnIndex, IntPtr rowIndex);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable32(IntPtr tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        //can throw an exception (at least in debug mode) on the c++ side if not columnindex < number of columns
        //could be handled by a check here, so that we never call if columindex <  number of coulms
        //note this also should work on mixed columns with subtables in them
        public static Table TableGetSubTable(Table parentTable,long columnIndex, long rowIndex)
        {
            if (columnIndex >= parentTable.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "GetSubTable called with column that is >= number of columns");
            }
            if (Is64Bit)
                return new Table(table_get_subtable64(parentTable.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex),true); //the constructor that takes an UintPtr will use that as a table handle
            else
                return new Table(table_get_subtable32(parentTable.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex),true); 
        }



        //tightdb_c_cs_API void unbind_table_ref(const size_t TablePtr)

        [DllImport("tightdb_c_cs64", EntryPoint = "unbind_table_ref", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref64(IntPtr tableHandle);
        [DllImport("tightdb_c_cs32", EntryPoint = "unbind_table_ref", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref32(IntPtr tableHandle);

//      void    table_unbind(const Table *t); /* Ref-count delete of table* from table_get_table() */
        public static void TableUnbind(Table t)
        {
            if (Is64Bit)
                unbind_table_ref64(t.TableHandle);
            else
                unbind_table_ref32(t.TableHandle);
            t.TableHandle = IntPtr.Zero;
        }


// tightdb_c_cs_API size_t table_get_spec(size_t TablePtr)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec64(IntPtr tableHandle);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec32(IntPtr tableHandle);


        //the spec returned here is live as long as the table itself is live, so don't dispose of the table and keep on using the spec
        public static Spec TableGetSpec(Table t)
        {           
            if(Is64Bit)  
            return new Spec(table_get_spec64(t.TableHandle),false);   //this spec should NOT be deallocated after use 
            else
            return new Spec(table_get_spec32(t.TableHandle),false);   //this spec should NOT be deallocated after use         
        }



        //tightdb_c_cs_API size_t get_column_count(tightdb::Table* TablePtr)


        //            size_t      table_get_column_count(const Table *t);        
        [DllImport("tightdb_c_cs64",EntryPoint="table_get_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_count64(IntPtr tableHandle);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_count32(IntPtr tableHandle);

        public static long TableGetColumnCount(Table t)
        {
            if (Is64Bit) 
                return (long)table_get_column_count64(t.TableHandle);
            else
                return (long)table_get_column_count32(t.TableHandle);
        }

        //table_get_column_name and spec_get_column_name have some buffer stuff in common. Consider refactoring. Note the DLL method call is
        //different in the two, and one function takes a spec, the other a table


        //tightdb_c_cs_API int get_column_name(Table* TablePtr,size_t column_ndx,char * colname, int bufsize)
        
        //Ignore comment below. Doesn't work wit MarshalAs for some reason
        //the MarshalAs LPTStr is set to let c# know that its 16 bit UTF-16 characters should be fit into a char* that uses 8 bit ANSI strings
        //In theory we *could* add a method to the DLL that takes and gives UTF-16 or similar, and a function that tells c# if UTF-16 is supported
        //on the c++ side on this platform. marshalling UTF-16 to UTF-16 will result in a buffer copy being saved, c++ would get a pointer directly into the stringbuilder buffer
        // [MarshalAs(UnmanagedType.LPTStr)]

        //            const char *table_get_column_name(const Table *t, size_t ndx);
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_column_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]       
        private static extern IntPtr table_get_column_name64(IntPtr tableHandle, IntPtr columnIndex, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_column_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr table_get_column_name32(IntPtr tableHandle, IntPtr columnIndex, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);


        public static string TableGetColumnName(Table t, long columnIndex)//ColumnIndex not a long bc on the c++ side it might be 32bit long on a 32 bit platform
        {
            var b = new StringBuilder(16);//string builder 16 is just a wild guess that most fields are shorter than this
            bool loop = true;
            do
            {
                int bufszneed;
                if (Is64Bit)
                    bufszneed = (int)table_get_column_name64(t.TableHandle, (IntPtr)columnIndex, b, (IntPtr)b.Capacity);//the intptr cast of the long *might* loose the high 32 bits on a 32 bits platform as tight c++ will only have support for 32 bit wide column counts on 32 bit
                else
                    bufszneed = (int)table_get_column_name32(t.TableHandle, (IntPtr)columnIndex, b, (IntPtr)b.Capacity);//the intptr cast of the long *might* loose the high 32 bits on a 32 bits platform as tight c++ will only have support for 32 bit wide column counts on 32 bit

                if (b.Capacity <= bufszneed)//Capacity is in .net chars, each of size 16 bits, while bufszneed is in bytes. HOWEVER stringbuilder often store common chars using only 8 bits, making precise calculations troublesome and slow
                {                           //what we know for sure is that the stringbuilder will hold AT LEAST capacity number of bytes. The c++ dll counts in bytes, so there is always room enough.
                    b.Capacity = bufszneed + 1;//allocate an array that is at least as large as what is needed, plus an extra 0 terminator.
                }
                else
                    loop = false;
            } while (loop);
            return b.ToString();//in c# this does NOT result in a copy, we get a string that points to the B buffer (If the now immutable string inside b is reused , it will get itself a new buffer
        }



        [DllImport("tightdb_c_cs64",EntryPoint="spec_get_column_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_get_column_name64(IntPtr specHandle, IntPtr columnIndex, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);
        [DllImport("tightdb_c_cs32", EntryPoint = "spec_get_column_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_get_column_name32(IntPtr specHandle, IntPtr columnIndex, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);


        public static String SpecGetColumnName(Spec spec, long columnIndex)
        {//see table_get_column_name for comments
            var b = new StringBuilder(16);
            bool loop = true;
            do
            {
                int bufszneed;
                if (Is64Bit)
                    bufszneed = (int)spec_get_column_name64(spec.SpecHandle, (IntPtr)columnIndex, b, (IntPtr)b.Capacity);
                else
                    bufszneed = (int)spec_get_column_name32(spec.SpecHandle, (IntPtr)columnIndex, b, (IntPtr)b.Capacity);


                if (b.Capacity <= bufszneed)
                {
                    b.Capacity = bufszneed + 1;
                }
                else
                    loop = false;
            } while (loop);

            return b.ToString();
        }



        //we have to trust that c++ DataType fills up the same amount of stack space as one of our own DataType enum's
        //This is the case on windows, visual studio2010 and 2012 but Who knows if some c++ compiler somewhere someday decides to store DataType differently
        //TODO: a more conservative approach would be to convert to size_t in c++ and to cast/convert it manually to DataType in c#
        //
        [DllImport("tightdb_c_cs64",EntryPoint="table_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType table_get_column_type64(IntPtr tablePtr, IntPtr columnIndex);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType table_get_column_type32(IntPtr tablePtr, IntPtr columnIndex);

        public static DataType TableGetColumnType(Table t, long columnIndex)
        {
            if (Is64Bit)
                return table_get_column_type64(t.TableHandle, (IntPtr)columnIndex);
            else
                return table_get_column_type32(t.TableHandle, (IntPtr)columnIndex);
        }


        //we have to trust that c++ DataType fills up the same amount of stack space as one of our own DataType enum's
        //This is the case on windows, visual studio2010 and 2012 but Who knows if some c++ compiler somewhere someday decides to store DataType differently
        //TODO: a more conservative approach would be to convert to size_t in c++ and to cast/convert it manually to DataType in c#
        //
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_mixed_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType table_get_mixed_type64(IntPtr tablePtr, IntPtr columnIndex,IntPtr rowIndex);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_mixed_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType table_get_mixed_type32(IntPtr tablePtr, IntPtr columnIndex, IntPtr rowIndex);

        public static DataType TableGetMixedType(Table t, long columnIndex,long rowIndex)
        {
            if (Is64Bit)
                return table_get_mixed_type64(t.TableHandle, (IntPtr)columnIndex,(IntPtr)rowIndex);
            else
                return table_get_mixed_type32(t.TableHandle, (IntPtr)columnIndex,(IntPtr)rowIndex);
        }






        [DllImport("tightdb_c_cs64", EntryPoint = "table_update_from_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_update_from_spec64(IntPtr tablePtr);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_update_from_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_update_from_spec32(IntPtr tablePtr);

        public static void TableUpdateFromSpec(Table table)
        {
            if (Is64Bit)
                table_update_from_spec64(table.TableHandle);
            else
                table_update_from_spec32(table.TableHandle);
        }


        //TIGHTDB_C_CS_API size_t table_add_empty_row(Table* TablePtr, size_t num_rows)

        [DllImport("tightdb_c_cs64", EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row64(IntPtr tablePtr, IntPtr numRows);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row32(IntPtr tablePtr, IntPtr numRows);

        public static long TableAddEmptyRow(Table table, long numberOfRows)
        {
            if (Is64Bit)
                return (long)table_add_empty_row64(table.TableHandle, (IntPtr)numberOfRows);
            else
                return (long)table_add_empty_row32(table.TableHandle, (IntPtr)numberOfRows);
        }


        //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_int64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_int32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);

        public static void TableSetLong(Table table, long columnIndex, long rowIndex, long value)
        {
#if DEBUG
            Log(MethodBase.GetCurrentMethod().Name, "(Table,Column,Row,Value)", table, columnIndex, rowIndex, value);
#endif

            if (Is64Bit)
                table_set_int64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_set_int32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }




        //TIGHTDB_C_CS_API void table_set_mixed_subtable(tightdb::Table* table_ptr,size_t col_ndx, size_t row_ndx,Table* source);
        [DllImport("tightdb_c_cs64", EntryPoint = "table_set_mixed_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_subtable64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr SourceTablePtr);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_set_mixed_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_subtable32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr SourceTablePtr);

        public static void TableSetMixedSubTable(Table table, long columnIndex, long rowIndex, Table sourceTable)
        {
#if DEBUG
            Log(MethodBase.GetCurrentMethod().Name, "(Table,Column,Row)", table, columnIndex, rowIndex);
#endif

            if (Is64Bit)
                table_set_mixed_subtable64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, sourceTable.TableHandle);
            else
                table_set_mixed_subtable32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, sourceTable.TableHandle);
        }

        [DllImport("tightdb_c_cs64", EntryPoint = "table_set_mixed_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_empty_subtable64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_set_mixed_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_empty_subtable32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        public static void TableSetMixedEmptySubTable(Table table, long columnIndex, long rowIndex)
        {
#if DEBUG
            Log(MethodBase.GetCurrentMethod().Name, "(Table,Column,Row)", table, columnIndex, rowIndex);
#endif

            if (Is64Bit)
                table_set_mixed_empty_subtable64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            else
                table_set_mixed_empty_subtable32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }






        [DllImport("tightdb_c_cs64", EntryPoint = "table_set_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_int64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_set_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_int32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);

        public static void TableSetMixedLong(Table table, long columnIndex, long rowIndex, long value)
        {
#if DEBUG
            Log(MethodBase.GetCurrentMethod().Name, "(Table,Column,Row,Value)", table, columnIndex, rowIndex, value);
#endif

            if (Is64Bit)
                table_set_mixed_int64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_set_mixed_int32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }



//        TIGHTDB_C_CS_API void insert_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_insert_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_int64(IntPtr tablePtr,IntPtr columnNdx, IntPtr rowNdx, long value);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_insert_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_int32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);

        public static void TableInsertInt(Table table, long columnIndex, long rowIndex, long value)
        {
#if DEBUG
            Log(MethodBase.GetCurrentMethod().Name,"(Table,Column,Row,Value)", table, columnIndex, rowIndex,value);
#endif

            if (Is64Bit)
                table_insert_int64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_insert_int32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }



        //TIGHTDB_C_CS_API int64_t table_get_int(Table* TablePtr, size_t column_ndx, size_t row_ndx)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_int64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_int32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);


        //note even though it's called getint - it does return an int64_t which is a 64 bit signed, that is, similar to C# long
        public static long TableGetInt(Table table, long columnIndex, long rowIndex)
        {
#if DEBUG
            long retval = Is64Bit ? table_get_int64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex) : table_get_int32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);

            Log(MethodBase.GetCurrentMethod().Name, "(Table,Column,Row,return)", table, columnIndex, rowIndex,retval);
            return retval;
#endif
// ReSharper disable CSharpWarnings::CS0162
// ReSharper disable HeuristicUnreachableCode
            return Is64Bit ? table_get_int64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex) : 
                             table_get_int32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
// ReSharper restore HeuristicUnreachableCode
// ReSharper restore CSharpWarnings::CS0162
        }


//get an int from a mixed
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_mixed_int64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_mixed_int32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);


        //note even though it's called getint - it does return an int64_t which is a 64 bit signed, that is, similar to C# long
        public static long TableGetMixedInt(Table table, long columnIndex, long rowIndex)
        {
#if DEBUG
            long retval = Is64Bit ? table_get_mixed_int64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex) : table_get_mixed_int32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);

            Log(MethodBase.GetCurrentMethod().Name, "(Table,Column,Row,return)", table, columnIndex, rowIndex, retval);
            return retval;
#endif
            // ReSharper disable CSharpWarnings::CS0162
            // ReSharper disable HeuristicUnreachableCode
            return Is64Bit ? table_get_mixed_int64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex) :
                             table_get_mixed_int32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore CSharpWarnings::CS0162
        }




        [DllImport("tightdb_c_cs64", EntryPoint = "table_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_size64(IntPtr tablePtr);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_size32(IntPtr tablePtr);

        public static long TableSize(Table t)
        {
            if (Is64Bit)
                return (long)table_get_size64(t.TableHandle);
            else
                return (long)table_get_size32(t.TableHandle);
        }


        //TIGHTDB_C_CS_API int64_t table_get_int(Table* TablePtr, size_t column_ndx, size_t row_ndx)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern sbyte table_get_bool64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);        
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern sbyte table_get_bool32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        public static bool TableGetBool(Table table, long columnIndex, long rowIndex)
        {
            if (Is64Bit)
                return Convert.ToBoolean(table_get_bool64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
            else
                return Convert.ToBoolean(table_get_bool32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
        }
    }
}