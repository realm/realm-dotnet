using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.Runtime.InteropServices;
//using System.Appdomain;


namespace TightDb.TightDbCSharp
{

    using System.Reflection;
    using System.IO;
    using System.Globalization;
    
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
        static bool is64bit 
        {
            get 
            {
                return (UIntPtr.Size == 8);//if this is evaluated every time, a faster way could be implemented. Size is cost when we are running though so perhaps it gets inlined by the JITter
            } 
        }

        static StringBuilder callog = null;//used to collect call data when debugging small unit tests

        static bool _loggingEnabled=false;
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
                    if (callog == null) { callog = new StringBuilder(); }
                    callog.AppendLine("--------LOGGING HAS BEEN ENABLED----------");
                    _loggingEnabled = true;
                }
                else
                {
                    callog.AppendLine("--------LOGGING HAS BEEN DISABLED----------");
                    _loggingEnabled = false;
                }
            }
        }

        static public void LoggingSaveFile(String fileName)
        {
            if (LoggingEnabled)
            {
                File.WriteAllText(fileName,callog.ToString());
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
                callog.AppendLine(String.Format(CultureInfo.InvariantCulture,"LOGGING ENABLED BY:{0}",marker));
            }
        }
        

        public static void log(string where,string desc, params  object[] values)
        {
            if (LoggingEnabled)
            {
                callog.Append(String.Format(CultureInfo.InvariantCulture,  "{0:yyyy-MM-dd HH:mm:ss} {1} {2}",DateTime.UtcNow, where,desc));
                callog.AppendLine(":");
                foreach (object o in values)
                {
                    string typestr = o.GetType().ToString();
                    string valuestr = o.ToString();
                    //if something doesn't auto.generate into readable code, we can test on o, and create custom more readable values
                    Type oType = o.GetType();
                    if (oType==typeof(Table))
                    {
                        valuestr = (o as Table).tableHandle.ToString();
                    }

                    callog.Append("Type(");
                    callog.Append(typestr);
                    callog.Append(") Value(");
                    callog.Append(valuestr);
                    callog.AppendLine(")");
                }
            }
            else
            { return; }
        }

        //tightdb_c_cs_API size_t tightdb_c_csGetVersion(void)
        [DllImport("tightdb_c_cs64",EntryPoint="tightdb_c_cs_GetVer" , CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr tightdb_c_cs_DllVer64();

        [DllImport("tightdb_c_cs32", EntryPoint = "tightdb_c_cs_GetVer", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr tightdb_c_cs_DllVer32();

        public static long CppDllVersion()
        {
            if (is64bit)
                return (long)tightdb_c_cs_DllVer64();
            else
                return (long)tightdb_c_cs_DllVer32();
        }
    

        [DllImport("tightdb_c_cs64", EntryPoint="spec_deallocate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate64(IntPtr spec);
        [DllImport("tightdb_c_cs32", EntryPoint = "spec_deallocate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate32(IntPtr spec);

        public static void SpecDeallocate(Spec s)
        {
            if (s.notifycppwhendisposing)//some spec's we get from C++ should not be deallocated by us
            {
                if (is64bit) 
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
            if(is64bit)
               return (long) spec_add_column64(spec.SpecHandle, type, name);
            else
                return (long)spec_add_column32(spec.SpecHandle, type, name);
        }


        [DllImport("tightdb_c_cs64", EntryPoint="table_add_column", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern UIntPtr table_add_column64(IntPtr tablehandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);
        [DllImport("tightdb_c_cs32",EntryPoint="table_add_column", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern UIntPtr table_add_column32(IntPtr tablehandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);

        public static long TableAddColumn(Table table, DataType type, string name)
        {
            if (is64bit)
                return (long)table_add_column64(table.tableHandle, type, name);//BM told me that column number sb long always in C#            
            else
                return (long)table_add_column32(table.tableHandle, type, name);//BM told me that column number sb long always in C#            
        }


        [DllImport("tightdb_c_cs64",EntryPoint="spec_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType spec_get_column_type64(IntPtr spechandle, IntPtr column_index);
        [DllImport("tightdb_c_cs32", EntryPoint="spec_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType spec_get_column_type32(IntPtr spechandle, IntPtr column_index);

        public static DataType SpecGetColumnType(Spec s, long column_index)
        {
            if (is64bit)
                return spec_get_column_type64(s.SpecHandle, (IntPtr)column_index);//the IntPtr cast of a long works on 32bit .net 4.5
            else
                return spec_get_column_type32(s.SpecHandle, (IntPtr)column_index);//the IntPtr cast of a long works on 32bit .net 4.5
        }

        /* not really needed
        public static DataType spec_get_column_type(Spec s, int column_index)
        {
            return spec_get_column_type(s.SpecHandle, (IntPtr)column_index);//the IntPtr cast of an int works on 32bit .net 4.5
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
            IntPtr SpecHandle;
            if (is64bit)
              SpecHandle = spec_add_subtable_column64(spec.SpecHandle, name);
            else
              SpecHandle = spec_add_subtable_column32(spec.SpecHandle, name);

            return new Spec(SpecHandle,true);//because this spechandle we get here should be deallocated
        }

        //get a spec given a column index. Returns specs for subtables, but not for mixed (as they would need a row index too)
        //Spec       *spec_get_spec(Spec *spec, size_t column_ndx);
        [DllImport("tightdb_c_cs64", EntryPoint="spec_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec64(IntPtr spec, IntPtr column_index);
        [DllImport("tightdb_c_cs32", EntryPoint = "spec_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec32(IntPtr spec, IntPtr column_index);



        private static string ErrColumnNotTable = "Spec_get_spec called with a column index for a column that is not a table";
        public static Spec SpecGetSpec(Spec spec, long columnIndex)
        {
            if (spec.GetColumnType(columnIndex) != DataType.Table)
            {
                throw new SpecException(ErrColumnNotTable);
            }            
            if (is64bit)
                return new Spec (spec_get_spec64((IntPtr)spec.SpecHandle, (IntPtr)columnIndex),true);                
            else
                return new Spec(spec_get_spec32((IntPtr)spec.SpecHandle, (IntPtr)columnIndex), true);            
        }

        /*not really needed
        public static Spec spec_get_spec(Spec spec, int column_idx)
        {
            if (spec.get_column_type(column_idx) == DataType.Table)
            {
                IntPtr SpecHandle = spec_get_spec((IntPtr)spec.SpecHandle, (IntPtr)column_idx);
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
            if (is64bit)
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
            if (is64bit)
                table.setTableHandle(new_table64()); //a call to table_new             
            else
                table.setTableHandle(new_table32()); //a call to table_new             
        }


        //tightdb_c_cs_API size_t new_table()
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable64(IntPtr tableHandle, IntPtr columnIndex, IntPtr rowIndex);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable32(IntPtr tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        //can throw an exception (at least in debug mode) on the c++ side if not columnindex < number of columns
        //could be handled by a check here, so that we never call if columindex <  number of coulms
        public static Table TableGetSubTable(Table parentTable,long columnIndex, long rowIndex)
        {
            if (columnIndex >= parentTable.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "GetSubTable called with column that is >= number of columns");
            }
            if (is64bit)
                return new Table(table_get_subtable64(parentTable.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)); //the constructor that takes an UintPtr will use that as a table handle
            else
                return new Table(table_get_subtable32(parentTable.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)); 
        }



        //tightdb_c_cs_API void unbind_table_ref(const size_t TablePtr)

        [DllImport("tightdb_c_cs64", EntryPoint = "unbind_table_ref", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref64(IntPtr TableHandle);
        [DllImport("tightdb_c_cs32", EntryPoint = "unbind_table_ref", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref32(IntPtr TableHandle);

//      void    table_unbind(const Table *t); /* Ref-count delete of table* from table_get_table() */
        public static void TableUnbind(Table t)
        {
            if (is64bit)
                unbind_table_ref64(t.tableHandle);
            else
                unbind_table_ref32(t.tableHandle);
            t.tableHandle = IntPtr.Zero;
        }


// tightdb_c_cs_API size_t table_get_spec(size_t TablePtr)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec64(IntPtr TableHandle);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec32(IntPtr TableHandle);


        //the spec returned here is live as long as the table itself is live, so don't dispose of the table and keep on using the spec
        public static Spec TableGetSpec(Table t)
        {           
            if(is64bit)  
            return new Spec(table_get_spec64(t.tableHandle),false);   //this spec should NOT be deallocated after use 
            else
            return new Spec(table_get_spec32(t.tableHandle), false);   //this spec should NOT be deallocated after use         
        }



        //tightdb_c_cs_API size_t get_column_count(tightdb::Table* TablePtr)


        //            size_t      table_get_column_count(const Table *t);        
        [DllImport("tightdb_c_cs64",EntryPoint="table_get_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_count64(IntPtr TableHandle);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_count32(IntPtr TableHandle);

        public static long TableGetColumnCount(Table t)
        {
            if (is64bit) 
                return (long)table_get_column_count64(t.tableHandle);
            else
                return (long)table_get_column_count32(t.tableHandle);
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
        private static extern IntPtr table_get_column_name64(IntPtr TableHandle, IntPtr column_idx, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_column_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr table_get_column_name32(IntPtr TableHandle, IntPtr column_idx, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);


        public static string TableGetColumnName(Table t, long column_idx)//column_idx not a long bc on the c++ side it might be 32bit long on a 32 bit platform
        {
            StringBuilder B = new StringBuilder(16);//string builder 16 is just a wild guess that most fields are shorter than this
            bool loop = true;
            int bufszneed;
            do
            {
                if (is64bit)
                    bufszneed = (int)table_get_column_name64(t.tableHandle, (IntPtr)column_idx, B, (IntPtr)B.Capacity);//the intptr cast of the long *might* loose the high 32 bits on a 32 bits platform as tight c++ will only have support for 32 bit wide column counts on 32 bit
                else
                    bufszneed = (int)table_get_column_name32(t.tableHandle, (IntPtr)column_idx, B, (IntPtr)B.Capacity);//the intptr cast of the long *might* loose the high 32 bits on a 32 bits platform as tight c++ will only have support for 32 bit wide column counts on 32 bit

                if (B.Capacity <= bufszneed)//Capacity is in .net chars, each of size 16 bits, while bufszneed is in bytes. HOWEVER stringbuilder often store common chars using only 8 bits, making precise calculations troublesome and slow
                {                           //what we know for sure is that the stringbuilder will hold AT LEAST capacity number of bytes. The c++ dll counts in bytes, so there is always room enough.
                    B.Capacity = bufszneed + 1;//allocate an array that is at least as large as what is needed, plus an extra 0 terminator.
                }
                else
                    loop = false;
            } while (loop);
            return B.ToString();//in c# this does NOT result in a copy, we get a string that points to the B buffer (If the now immutable string inside b is reused , it will get itself a new buffer
        }



        [DllImport("tightdb_c_cs64",EntryPoint="spec_get_column_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_get_column_name64(IntPtr SpecHandle, IntPtr column_idx, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);
        [DllImport("tightdb_c_cs32", EntryPoint = "spec_get_column_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_get_column_name32(IntPtr SpecHandle, IntPtr column_idx, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);


        public static String SpecGetColumnName(Spec spec, long column_idx)
        {//see table_get_column_name for comments
            StringBuilder B = new StringBuilder(16);
            bool loop = true;
            int bufszneed;
            do
            {
                if (is64bit)
                    bufszneed = (int)spec_get_column_name64(spec.SpecHandle, (IntPtr)column_idx, B, (IntPtr)B.Capacity);
                else
                    bufszneed = (int)spec_get_column_name32(spec.SpecHandle, (IntPtr)column_idx, B, (IntPtr)B.Capacity);


                if (B.Capacity <= bufszneed)
                {
                    B.Capacity = bufszneed + 1;
                }
                else
                    loop = false;
            } while (loop);

            return B.ToString();
        }



        
        //    TightdbDataType table_get_column_type(const Table *t, size_t ndx);
        [DllImport("tightdb_c_cs64",EntryPoint="table_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType table_get_column_type64(IntPtr TablePtr, IntPtr column_idx);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType table_get_column_type32(IntPtr TablePtr, IntPtr column_idx);

        public static DataType TableGetColumnType(Table t, long column_idx)
        {
            if (is64bit)
                return table_get_column_type64(t.tableHandle, (IntPtr)column_idx);
            else
                return table_get_column_type32(t.tableHandle, (IntPtr)column_idx);
        }





        [DllImport("tightdb_c_cs64", EntryPoint = "table_update_from_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_update_from_spec64(IntPtr TablePtr);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_update_from_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_update_from_spec32(IntPtr TablePtr);

        public static void TableUpdateFromSpec(Table table)
        {
            if (is64bit)
                table_update_from_spec64(table.tableHandle);
            else
                table_update_from_spec32(table.tableHandle);
        }


        //TIGHTDB_C_CS_API size_t table_add_empty_row(Table* TablePtr, size_t num_rows)

        [DllImport("tightdb_c_cs64", EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row64(IntPtr TablePtr, IntPtr num_rows);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row32(IntPtr TablePtr, IntPtr num_rows);

        public static long TableAddEmptyRow(Table table, long NumberOfRows)
        {
            if (is64bit)
                return (long)table_add_empty_row64(table.tableHandle, (IntPtr)NumberOfRows);
            else
                return (long)table_add_empty_row32(table.tableHandle, (IntPtr)NumberOfRows);
        }


        //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_int64(IntPtr TablePtr, IntPtr column_ndx, IntPtr row_ndx, long value);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_int32(IntPtr TablePtr, IntPtr column_ndx, IntPtr row_ndx, long value);

        public static void TableSetLong(Table table, long columnIndex, long rowIndex, long value)
        {
#if DEBUG
            log(MethodInfo.GetCurrentMethod().Name, "(Table,Column,Row,Value)", table, columnIndex, rowIndex, value);
#endif

            if (is64bit)
                table_set_int64(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_set_int32(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }



//        TIGHTDB_C_CS_API void insert_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_insert_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_int64(IntPtr TablePtr,IntPtr column_ndx, IntPtr row_ndx, long value);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_insert_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_int32(IntPtr TablePtr, IntPtr column_ndx, IntPtr row_ndx, long value);

        public static void TableInsertInt(Table table, long columnIndex, long rowIndex, long value)
        {
#if DEBUG
            log(MethodInfo.GetCurrentMethod().Name,"(Table,Column,Row,Value)", table, columnIndex, rowIndex,value);
#endif

            if (is64bit)
                table_insert_int64(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_insert_int32(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }



        //TIGHTDB_C_CS_API int64_t table_get_int(Table* TablePtr, size_t column_ndx, size_t row_ndx)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_int64(IntPtr TablePtr, IntPtr column_ndx, IntPtr row_ndx);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_int32(IntPtr TablePtr, IntPtr column_ndx, IntPtr row_ndx);


        //note even though it's called getint - it does return an int64_t which is a 64 bit signed, that is, similar to C# long
        public static long TableGetInt(Table table, long columnIndex, long rowIndex)
        {
#if DEBUG  
            long retval;
            if (is64bit)
                retval= (long)table_get_int64(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            else
                retval = (long)table_get_int32(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);

            log(MethodInfo.GetCurrentMethod().Name, "(Table,Column,Row,return)", table, columnIndex, rowIndex,retval);
            return retval;
#endif
            if (is64bit)
                return (long)table_get_int64(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            else
                return (long)table_get_int32(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }

        [DllImport("tightdb_c_cs64", EntryPoint = "table_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_size64(IntPtr TablePtr);
        [DllImport("tightdb_c_cs32", EntryPoint = "table_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_size32(IntPtr TablePtr);

        public static long TableSize(Table t)
        {
            if (is64bit)
                return (long)table_get_size64(t.tableHandle);
            else
                return (long)table_get_size32(t.tableHandle);
        }


        //TIGHTDB_C_CS_API int64_t table_get_int(Table* TablePtr, size_t column_ndx, size_t row_ndx)
        [DllImport("tightdb_c_cs64", EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern sbyte table_get_bool64(IntPtr TablePtr, IntPtr column_ndx, IntPtr row_ndx);        
        [DllImport("tightdb_c_cs32", EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern sbyte table_get_bool32(IntPtr TablePtr, IntPtr column_ndx, IntPtr row_ndx);

        public static bool TableGetBool(Table table, long columnIndex, long rowIndex)
        {
            if (is64bit)
                return Convert.ToBoolean(table_get_bool64(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
            else
                return Convert.ToBoolean(table_get_bool32(table.tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
        }
    }
}