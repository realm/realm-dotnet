using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

//WARNING - this is an old file, it was coded using the old C bindings as a reference point.
//All the code in here will undergo major changes
//except a few methods that actually call the newer c++ dll
//the old code can be recognized by its use of a method calle fakepinvoke

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

    //Used exclusively in table constructors
    public enum TDB
    {
        Int = 0,
        Bool = 1,
        String = 2,
        Binary = 4,
        Table = 5,
        Mixed = 6,
        Date = 7,
        type_Float  =  9,
       type_Double = 10
    }
   
    public enum TightDbDataType
    {
        Int = 0,
        Bool = 1,
        String = 2,
        Binary = 4,
        Table = 5,
        Mixed = 6,
        Date = 7,
        type_Float = 9,
        type_Double = 10
    }

    //this class contains methods for calling the c++ TightDB system, which has been flattened out as C type calls   
    //The individual methods call the C inteface using types and values suitable for the C interface, but the methods take and give
    //values that are suitable for C# 
    //all the stuff that is sent to TightDB c++ via P/Invoce is prefixed TDB so a C# spec is called spec while the 
    //value holding the c++ address of the real c++ hosted spec is called SpecHandle
    class TightDBCalls
    {

        /**debugging an d system info calls**/
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
            if (s.notifycppwhendisposing)
            {
                spec_deallocate(s.SpecHandle);
                s.SpecHandle = IntPtr.Zero;
            }
        }

// TIGHTCSDLL_API size_t add_column(size_t SpecPtr,DataType type, const char* name) 

        //marshalling : not sure the enum has the same size on C# and c++ on all platforms and bit sizes
        //and not sure if the marshaller will fix it for us if they are not of the same size
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr spec_add_column(IntPtr spec, TDB type, string name);
       

        public static void spec_add_column(Spec spec, TDB type, string name)
        {                   
            spec_add_column(spec.SpecHandle, type, name);//wonder if this works
            //vaules to send : SpecHandle, type, name
        }

        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern TDB spec_get_column_type(IntPtr spechandle, IntPtr column_index);

        public static TDB spec_get_column_type(Spec s, long column_index)
        {
            return spec_get_column_type(s.SpecHandle, (IntPtr)column_index);
        }

        //Spec add_subtable_column(const char* name);
        //also see 
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_add_subtable_column(IntPtr spec,string name);

        public static Spec add_subtable_column(Spec s,String name)
        {
            IntPtr SpecHandle = spec_add_subtable_column(s.SpecHandle, name);
            return new Spec(SpecHandle,true);//because this spechandle we get here should be deallocated
        }

/*
        public static Spec spec_add_column_table(Spec spec, string name)
        {
            Spec r = new Spec();
            //add pinvoke call and mashalling to  Spec       *spec_add_column_table(Spec *spec, const char *name);            
            r.SpecHandle = (UIntPtr)fakepinvoke(spec, name);
            return r;
        }
        */

        //column indexes in C# for specs are ints 
        //Spec       *spec_get_spec(Spec *spec, size_t column_ndx);
        /*
        public static Spec spec_get_spec(Spec spec, long column_idx)
        {
            
            UIntPtr TDBColix = (IntPtr)column_idx;//should safely set the TDBColix to the value of the int index            
            
              return new Spec((IntPtr)fakepinvoke(spec.SpecHandle, TDBColix));
            
        }*/


        //size_t spec_get_column_count(Spec* spec);
        public static long spec_get_column_count(Spec spec)
        {            
            UIntPtr res = (UIntPtr)fakepinvoke(spec.SpecHandle);        //size_t spec_get_column_count(Spec* spec);
            return (long)res;
        }





        // const char *spec_get_column_name(Spec *spec, size_t column_idx);
        public static String spec_get_column_name(Spec spec, long column_idx)
        {
            UIntPtr TDBcolix = (UIntPtr)column_idx;
            String res = (string)fakepinvoke(spec.SpecHandle, TDBcolix);
            return res;
        }


        //    size_t      spec_get_column_index(Spec *spec, const char *name);

        public static long spec_get_column_index(Spec spec, String name)
        {            
            UIntPtr col_idx = (UIntPtr)fakepinvoke(spec.SpecHandle, name);
            long res = (long)col_idx;
            return res;
        }

/*
        //methods here below are not in use atm.
        // c code :    size_t      spec_get_ref(Spec *spec);
        // expected functionality : unknown (seems to take a pointer to a SpecHandle and return a pointer sized int)
        // Not sure if that pointer sized int is a pointer to a spec or what it is
        /*
        private static Spec spec_get_ref(Spec spec)
        {
            //add pinvoke call and marshalling to :     size_t      spec_get_ref(Spec *spec);            
            UIntPtr SpecHandleresult  = (UIntPtr)fakepinvoke(spec.SpecHandle);

            return new Spec(SpecHandleresult);
        }
        */

        /*** Table ************************************/

 //TIGHTCSDLL_API size_t new_table()
        //this dll must be manually copied to the location of the testpinvoke program exefile
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


 //       void table_update_from_spec(Table* t);
        public static void table_update_from_spec(Table t)
        {
            
            fakepinvoke(t.TableHandle);
        }


        //TIGHTCSDLL_API size_t get_column_count(tightdb::Table* TablePtr)

        public static long table_register_column(Table t, TDB type, string name)
        {            
            UIntPtr col_idx = (UIntPtr) fakepinvoke(t.TableHandle, type, name);
            return (long)col_idx;            
        }

        //            size_t      table_get_column_count(const Table *t);        
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_count(IntPtr TableHandle);

        public static long table_get_column_count(Table t)
        {
            IntPtr column_cnt = table_get_column_count(t.TableHandle);
            return (long)column_cnt;
        }

//            size_t      table_get_column_index(const Table *t, const char *name);
        public static long table_get_column_index(Table t, string name)
        {            
            UIntPtr column_idx = (UIntPtr)fakepinvoke(t.TableHandle, name);
            return (long)column_idx;
        }

        //TIGHTCSDLL_API int get_column_name(Table* TablePtr,size_t column_ndx,char * colname, int bufsize)
        //the MarshalAs LPTStr is set to let c# know that its 16 bit UTF-16 characters should be fit into a char* that uses 8 bit ANSI strings
        //In theory we *could* add a method to the DLL that takes and gives UTF-16 or similar, and a function that tells c# if UTF-16 is supported
        //on the c++ side on this platform. marshalling UTF-16 to UTF-16 will result in a buffer copy being saved, c++ would get a pointer directly into the stringbuilder buffer
        // [MarshalAs(UnmanagedType.LPTStr)]
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr table_get_column_name(IntPtr TableHandle, IntPtr column_idx, StringBuilder name, IntPtr bufsize);


//            const char *table_get_column_name(const Table *t, size_t ndx);

        public static string table_get_column_name(Table t, long column_idx)//column_idx not a long bc on the c++ side it might be 32bit long on a 32 bit platform
        {
            StringBuilder B = new StringBuilder(16);//string builder 16 is just a wild guess that most fields are shorter than this
            bool loop = true;
            while (loop)
            {
                int bufszneed=(int)table_get_column_name(t.TableHandle, (IntPtr)column_idx, B, (IntPtr)B.Capacity + 1);//the intptr cast of the long *might* loose the high 32 bits on a 32 bits platform as tight c++ will only have support for 32 bit wide column counts on 32 bit
                if (B.Capacity <= bufszneed )//Capacity is in .net chars, each filling 16 bits, while bufszneed is in bytes. 
                {
                    B.Capacity = bufszneed + 1;//so here we allocate an array that is way larger than what is likely probably needed.
                } else
                    loop=false;//ok to send capacity as marshalling will
            }
            return B.ToString();//in c# this does NOT result in a copy, we get a string that points to the B buffer (If the now immutable string inside b is reused , it will get itself a new buffer
        }


        
        //    TightdbDataType table_get_column_type(const Table *t, size_t ndx);
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern TDB table_get_column_type(IntPtr TablePtr, IntPtr column_idx);

        public static TDB table_get_column_type(Table t, long column_idx)
        {
            return table_get_column_type(t.TableHandle,(IntPtr)column_idx);
        }


//             void table_add(Table *t, ...);
        //I assume this one adds an empty colum to the end of the table and returns the ix of that row
        public static long table_add(Table t)
        {
            UIntPtr colix = (UIntPtr)fakepinvoke(t.TableHandle);
            return (long)colix;
        }


        //             void table_add(Table *t, ...);
        //I assume this one adds an empty colum to the end of the table and returns the ix of that row
        public static long table_insert(Table t,long col_idx)
        {
            UIntPtr colix = (UIntPtr)fakepinvoke(t.TableHandle);
            return (long)colix;
        }

        public static TightDbDataType table_get_columntype(Table t, long columnindex)
        {
           UIntPtr datatype =(UIntPtr)fakepinvoke(t.TableHandle,columnindex);
           return (TightDbDataType)datatype;
        }

        public static long table_get_int(Table t ,long ColumnIndex,long RecordIndex)
        {
            UIntPtr res = (UIntPtr)fakepinvoke(t.TableHandle, ColumnIndex, RecordIndex);
            return (long)res;
        }

        /* Getting values */
        //    int64_t     table_get_int(const Table *t, size_t column_ndx, size_t ndx);
        //    bool        table_get_bool(const Table *t, size_t column_ndx, size_t ndx);
        //    time_t      table_get_date(const Table *t, size_t column_ndx, size_t ndx);
        //    const char *table_get_string(const Table *t, size_t column_ndx, size_t ndx);
        //    BinaryData *table_get_binary(const Table *t, size_t column_ndx, size_t ndx);
        //    Mixed      *table_get_mixed(const Table *t, size_t column_ndx, size_t ndx);
        //   TightdbDataType table_get_mixed_type(const Table *t, size_t column_ndx, size_t ndx);
        //    Table       *table_get_subtable(Table *t, size_t column_ndx, size_t ndx);
        //    const Table *table_get_const_subtable(const Table *t, size_t column_ndx, size_t ndx);
        /* Use table_unbind() to 'delete' the table after use */


        //not used - enables us to write code that resemples the external methods to be created later
        //fakepinvoke can be calld with any number of parametres of any type and return any kind of object.
        private static Object fakepinvoke(params object[] parametres) { return null; }


    }
}