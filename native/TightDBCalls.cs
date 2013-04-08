using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks; not portable
using System.Runtime.InteropServices;

//WARNING - this is an old file, it was coded using the old C bindings as a reference point.
//All the code in here will undergo major changes
//except a few methods that actually call the newer c++ dll

namespace tightdb.Tightdbcsharp
{
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

    public enum TDB
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
    class TightDBCalls
    {

        /**debugging and system info calls**/
        //TIGHTCSDLL_API size_t tightCSDLLGetVersion(void)
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr tightCSDLLGetVersion();

        public static long tightCSDLLVersion() {
            return (long)tightCSDLLGetVersion();
        }

        /*** Spec ************************************/


        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate(IntPtr spec);

        public static void spec_deallocate(Spec s)
        {
            if (s.notifycppwhendisposing)//some spec's we get from C++ should not be deallocated by us
            {
                spec_deallocate(s.SpecHandle);
                s.SpecHandle = IntPtr.Zero;
            }
        }

// TIGHTCSDLL_API size_t add_column(size_t SpecPtr,DataType type, const char* name) 

        //marshalling : not sure the simple enum members have the same size on C# and c++ on all platforms and bit sizes
        //and not sure if the marshaller will fix it for us if they are not of the same size
        //so this must be tested on various platforms and bit sizes, and perhaps specific versions of calls with enums have to be made
        //this one works on windows 7, .net 4.5 32 bit, tightdb 32 bit (on a 64 bit OS, but that shouldn't make a difference)
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr spec_add_column(IntPtr spechandle, TDB type, string name);
      
        public static void spec_add_column(Spec spec, TDB type, string name)
        {                   
            spec_add_column(spec.SpecHandle, type, name);            
        }

        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern TDB spec_get_column_type(IntPtr spechandle, IntPtr column_index);

        public static TDB spec_get_column_type(Spec s, long column_index)
        {
            return spec_get_column_type(s.SpecHandle, (IntPtr)column_index);//the IntPtr cast of a long works on 32bit .net 4.5
        }

        public static TDB spec_get_column_type(Spec s, int column_index)
        {
            return spec_get_column_type(s.SpecHandle, (IntPtr)column_index);//the IntPtr cast of an int works on 32bit .net 4.5
        }                                                                   //but probably throws an exception or warning on 64bit 

        //Spec add_subtable_column(const char* name);        
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_add_subtable_column(IntPtr spec,string name);

        public static Spec add_subtable_column(Spec s,String name)
        {
            IntPtr SpecHandle = spec_add_subtable_column(s.SpecHandle, name);
            return new Spec(SpecHandle,true);//because this spechandle we get here should be deallocated
        }

        //get a spec given a column index. Returns specs for subtables, but not for mixed (as they would need a row index too)
        //Spec       *spec_get_spec(Spec *spec, size_t column_ndx);
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec(IntPtr spec, IntPtr column_index);

        private static string err_column_not_table = "Spec_get_spec called with a column index that does not contain a table field";
        public static Spec spec_get_spec(Spec spec, long column_idx)
        {
            if (spec.get_column_type(column_idx)==TDB.Table)
            {
                IntPtr SpecHandle = spec_get_spec((IntPtr)spec.SpecHandle, (IntPtr)column_idx);
                return new Spec(SpecHandle,true);
            }
            else
                throw new SpecException(err_column_not_table);
        }

        public static Spec spec_get_spec(Spec spec, int column_idx)
        {
            if (spec.get_column_type(column_idx) == TDB.Table)
            {
                IntPtr SpecHandle = spec_get_spec((IntPtr)spec.SpecHandle, (IntPtr)column_idx);
                return new Spec(SpecHandle, true);
            }
            else
                throw new SpecException(err_column_not_table);
        }


        //size_t spec_get_column_count(Spec* spec);
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_count(IntPtr spec);

        public static long spec_get_column_count(Spec spec)
        {
            return (long) spec_get_column_count(spec.SpecHandle);//OPTIMIZE  - could be optimized with a 32 or 64 bit specific implementation - see end of file
        }






        /*** Table ************************************/

        //TIGHTCSDLL_API size_t new_table()
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr new_table(); 

        public static void  table_new(Table table)
        {            
            table.TableHandle = (IntPtr)new_table(); //a call to table_new 
        }

        //TIGHTCSDLL_API void unbind_table_ref(const size_t TablePtr)

        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref(IntPtr TableHandle);

//      void    table_unbind(const Table *t); /* Ref-count delete of table* from table_get_table() */
        public static void table_unbind(Table t)
        {
            unbind_table_ref(t.TableHandle);
            t.TableHandle = IntPtr.Zero;            
        }


// TIGHTCSDLL_API size_t table_get_spec(size_t TablePtr)
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec(IntPtr TableHandle);


        //the spec returned here is live as long as the table itself is live, so don't dispose of the table and keep on using the spec
        public static Spec table_get_spec(Table t)
        {                         
            return new Spec(table_get_spec(t.TableHandle),false);   //this spec should NOT be deallocated after use         
        }



        //TIGHTCSDLL_API size_t get_column_count(tightdb::Table* TablePtr)


        //            size_t      table_get_column_count(const Table *t);        
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_count(IntPtr TableHandle);

        public static long table_get_column_count(Table t)
        {
            IntPtr column_cnt = table_get_column_count(t.TableHandle);
            return (long)column_cnt;
        }


        //table_get_column_name and spec_get_column_name have some buffer stuff in common. Consider refactoring. Note the DLL method call is
        //different in the two, and one function takes a spec, the other a table


        //TIGHTCSDLL_API int get_column_name(Table* TablePtr,size_t column_ndx,char * colname, int bufsize)
        
        //Ignore comment below. Doesn't work wit MarshalAs for some reason
        //the MarshalAs LPTStr is set to let c# know that its 16 bit UTF-16 characters should be fit into a char* that uses 8 bit ANSI strings
        //In theory we *could* add a method to the DLL that takes and gives UTF-16 or similar, and a function that tells c# if UTF-16 is supported
        //on the c++ side on this platform. marshalling UTF-16 to UTF-16 will result in a buffer copy being saved, c++ would get a pointer directly into the stringbuilder buffer
        // [MarshalAs(UnmanagedType.LPTStr)]

        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        //            const char *table_get_column_name(const Table *t, size_t ndx);
        public static extern IntPtr table_get_column_name(IntPtr TableHandle, IntPtr column_idx, StringBuilder name, IntPtr bufsize);


        public static string table_get_column_name(Table t, long column_idx)//column_idx not a long bc on the c++ side it might be 32bit long on a 32 bit platform
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



        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr spec_get_column_name(IntPtr SpecHandle, IntPtr column_idx, StringBuilder name, IntPtr bufsize);
        public static String spec_get_column_name(Spec spec, long column_idx)
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
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern TDB table_get_column_type(IntPtr TablePtr, IntPtr column_idx);

        public static TDB table_get_column_type(Table t, long column_idx)
        {
            return table_get_column_type(t.TableHandle,(IntPtr)column_idx);
        }







    }
}