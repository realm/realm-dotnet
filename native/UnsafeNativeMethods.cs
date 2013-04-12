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
        
        //tightdb_c_cs_API size_t tightdb_c_csGetVersion(void)
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr tightdb_c_cs_GetVersion();

        public static long CppDllVersion() {
            return (long)tightdb_c_cs_GetVersion();
        }

        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate(IntPtr spec);

        public static void SpecDeallocate(Spec s)
        {
            if (s.notifycppwhendisposing)//some spec's we get from C++ should not be deallocated by us
            {
                spec_deallocate(s.SpecHandle);
                s.SpecHandle = IntPtr.Zero;
            }
        }




// tightdb_c_cs_API size_t add_column(size_t SpecPtr,DataType type, const char* name) 

        //marshalling : not sure the simple enum members have the same size on C# and c++ on all platforms and bit sizes
        //and not sure if the marshaller will fix it for us if they are not of the same size
        //so this must be tested on various platforms and bit sizes, and perhaps specific versions of calls with enums have to be made
        //this one works on windows 7, .net 4.5 32 bit, tightdb 32 bit (on a 64 bit OS, but that shouldn't make a difference)
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern UIntPtr spec_add_column(IntPtr spechandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);
      
        public static long SpecAddColumn(Spec spec, DataType type, string name)
        {                   
            return (long) spec_add_column(spec.SpecHandle, type, name);
        }

        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern UIntPtr table_add_column(IntPtr tablehandle, DataType type, [MarshalAs(UnmanagedType.LPStr)]string name);

        public static long TableAddColumn(Table table, DataType type, string name)
        {
            return (long)table_add_column(table.TableHandle, type, name);//BM told me that column number sb long always in C#            
        }


        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType spec_get_column_type(IntPtr spechandle, IntPtr column_index);

        public static DataType SpecGetColumnType(Spec s, long column_index)
        {
            return spec_get_column_type(s.SpecHandle, (IntPtr)column_index);//the IntPtr cast of a long works on 32bit .net 4.5
        }

        /* not really needed
        public static DataType spec_get_column_type(Spec s, int column_index)
        {
            return spec_get_column_type(s.SpecHandle, (IntPtr)column_index);//the IntPtr cast of an int works on 32bit .net 4.5
        }                                                                   //but probably throws an exception or warning on 64bit 
        */
        //Spec add_subtable_column(const char* name);        
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_add_subtable_column(IntPtr spec,[MarshalAs(UnmanagedType.LPStr)] string name);

        public static Spec AddSubTableColumn(Spec spec,String name)
        {
            IntPtr SpecHandle = spec_add_subtable_column(spec.SpecHandle, name);
            return new Spec(SpecHandle,true);//because this spechandle we get here should be deallocated
        }

        //get a spec given a column index. Returns specs for subtables, but not for mixed (as they would need a row index too)
        //Spec       *spec_get_spec(Spec *spec, size_t column_ndx);
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec(IntPtr spec, IntPtr column_index);

        private static string ErrColumnNotTable = "Spec_get_spec called with a column index that does not contain a table field";
        public static Spec SpecGetSpec(Spec spec, long columnIndex)
        {
            if (spec.GetColumnType(columnIndex)==DataType.Table)
            {
                IntPtr SpecHandle = spec_get_spec((IntPtr)spec.SpecHandle, (IntPtr)columnIndex);
                return new Spec(SpecHandle,true);
            }
            else
                throw new SpecException(ErrColumnNotTable);
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
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_count(IntPtr spec);

        public static long SpecGetColumnCount(Spec spec)
        {
            return (long) spec_get_column_count(spec.SpecHandle);//OPTIMIZE  - could be optimized with a 32 or 64 bit specific implementation - see end of file
        }






        /*** Table ************************************/

        //tightdb_c_cs_API size_t new_table()
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr new_table(); 

        
        public static void  TableNew(Table table)
        {
                table.TableHandle = (IntPtr)new_table(); //a call to table_new             
        }

        //tightdb_c_cs_API void unbind_table_ref(const size_t TablePtr)

        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref(IntPtr TableHandle);

//      void    table_unbind(const Table *t); /* Ref-count delete of table* from table_get_table() */
        public static void TableUnbind(Table t)
        {
            unbind_table_ref(t.TableHandle);
            t.TableHandle = IntPtr.Zero;            
        }


// tightdb_c_cs_API size_t table_get_spec(size_t TablePtr)
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec(IntPtr TableHandle);


        //the spec returned here is live as long as the table itself is live, so don't dispose of the table and keep on using the spec
        public static Spec TableGetSpec(Table t)
        {                         
            return new Spec(table_get_spec(t.TableHandle),false);   //this spec should NOT be deallocated after use         
        }



        //tightdb_c_cs_API size_t get_column_count(tightdb::Table* TablePtr)


        //            size_t      table_get_column_count(const Table *t);        
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_count(IntPtr TableHandle);

        public static long TableGetColumnCount(Table t)
        {
            IntPtr column_cnt = table_get_column_count(t.TableHandle);
            return (long)column_cnt;
        }


        //table_get_column_name and spec_get_column_name have some buffer stuff in common. Consider refactoring. Note the DLL method call is
        //different in the two, and one function takes a spec, the other a table


        //tightdb_c_cs_API int get_column_name(Table* TablePtr,size_t column_ndx,char * colname, int bufsize)
        
        //Ignore comment below. Doesn't work wit MarshalAs for some reason
        //the MarshalAs LPTStr is set to let c# know that its 16 bit UTF-16 characters should be fit into a char* that uses 8 bit ANSI strings
        //In theory we *could* add a method to the DLL that takes and gives UTF-16 or similar, and a function that tells c# if UTF-16 is supported
        //on the c++ side on this platform. marshalling UTF-16 to UTF-16 will result in a buffer copy being saved, c++ would get a pointer directly into the stringbuilder buffer
        // [MarshalAs(UnmanagedType.LPTStr)]

        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        //            const char *table_get_column_name(const Table *t, size_t ndx);
        private static extern IntPtr table_get_column_name(IntPtr TableHandle, IntPtr column_idx, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);


        public static string TableGetColumnName(Table t, long column_idx)//column_idx not a long bc on the c++ side it might be 32bit long on a 32 bit platform
        {
            StringBuilder B = new StringBuilder(16);//string builder 16 is just a wild guess that most fields are shorter than this
            bool loop = true;
            do
            {
                int bufszneed = (int)table_get_column_name(t.TableHandle, (IntPtr)column_idx, B, (IntPtr)B.Capacity);//the intptr cast of the long *might* loose the high 32 bits on a 32 bits platform as tight c++ will only have support for 32 bit wide column counts on 32 bit
                if (B.Capacity <= bufszneed)//Capacity is in .net chars, each of size 16 bits, while bufszneed is in bytes. HOWEVER stringbuilder often store common chars using only 8 bits, making precise calculations troublesome and slow
                {                           //what we know for sure is that the stringbuilder will hold AT LEAST capacity number of bytes. The c++ dll counts in bytes, so there is always room enough.
                    B.Capacity = bufszneed + 1;//allocate an array that is at least as large as what is needed, plus an extra 0 terminator.
                }
                else
                    loop = false;
            } while (loop);
            return B.ToString();//in c# this does NOT result in a copy, we get a string that points to the B buffer (If the now immutable string inside b is reused , it will get itself a new buffer
        }



        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr spec_get_column_name(IntPtr SpecHandle, IntPtr column_idx, [MarshalAs(UnmanagedType.LPStr)]StringBuilder name, IntPtr bufsize);
        public static String SpecGetColumnName(Spec spec, long column_idx)
        {//see table_get_column_name for comments
            StringBuilder B = new StringBuilder(16);
            bool loop = true;
            do
            {
                int bufszneed = (int)spec_get_column_name(spec.SpecHandle, (IntPtr)column_idx, B, (IntPtr)B.Capacity);
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
        [DllImport("tightdb_c_cs", CallingConvention = CallingConvention.Cdecl)]
        private static extern DataType table_get_column_type(IntPtr TablePtr, IntPtr column_idx);

        public static DataType TableGetColumnType(Table t, long column_idx)
        {
            return table_get_column_type(t.TableHandle,(IntPtr)column_idx);
        }





        [DllImport("tightdb_c_cs", CallingConvention=CallingConvention.Cdecl)]
        private static extern void table_update_from_spec(IntPtr TablePtr);

        public static void TableUpdateFromSpec(Table table)
        {
            table_update_from_spec(table.TableHandle);
        }
    }
}