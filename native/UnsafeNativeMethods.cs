using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
#if V45PLUS
  //unity will not use an assembly that uses compilerservices
  //and currently unity runs net35
  //We only use compilerservices to add agressiveinlining which is only supported in 45 so only use compilerservices
  //if we are building a NERT45 library
  using System.Runtime.CompilerServices;
#endif
using System.Text;


namespace TightDbCSharp
{
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

    /// <summary>
    /// Enumerates the types of columns a TightDb table can have
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Indicates a TighDb type_Int column
        /// </summary>

        Int = 0,

        /// <summary>
        /// Indicates a TighDb type_Bool column
        /// </summary>
        Bool = 1,

        /// <summary>
        /// Indicates a TighDb type_string column
        /// </summary>
        String = 2,

        /// <summary>
        /// Indicates a TighDb type_Binary column
        /// </summary>
        Binary = 4,

        /// <summary>
        /// Indicates a TighDb type_Table column
        /// </summary>
        Table = 5,

        /// <summary>
        /// Indicates a TighDb type_Mixed column
        /// </summary>
        Mixed = 6,

        /// <summary>
        /// Indicates a TighDb type_Date column
        /// </summary>
        Date = 7,

        /// <summary>
        /// Indicates a TighDb type_Float column
        /// </summary>
        Float = 9,

        /// <summary>
        /// Indicates a TighDb type_Double column
        /// </summary>

        Double = 10
    }






    //this class contains methods for calling the c++ TightDB system, which has been flattened out as C type calls   
    //The individual public methods call the C inteface using types and values suitable for the C interface, but the methods take and give
    //values that are suitable for C# (for instance taking a C# Table object parameter and calling on with the C++ Table pointer inside the C# Table Class)

    //const groups and their childs are handled here , readonly is only set in methods in this file.
    //in c++ readonly is usually passed as a parameter if the c++ call must differ depending on the c++ class is readonly or not

#if V40PLUS
#else
//        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
#endif

    public static class UnsafeNativeMethods
    {



        //http://connect.microsoft.com/VisualStudio/feedback/details/729254/bogus-ca5122-warning-about-p-invoke-declarations-should-not-be-saf
        //above confirms that CA5122 fires wrongly bc of a bug in code analysis. This one call has been kept to keep us
        //aware of the fact, all the other warnings have been disabled.
        //if this method does NOT give a CA5122 when built for .net version 3.5 then msft have fixed their fxcop bug
        //and the ca5122 suppressions in the suppresion files should be removed

        //tightdb_c_cs_API size_t tightdb_c_csGetVersion(void)
        [DllImport(L64, EntryPoint = "tightdb_c_cs_getver", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tightdb_c_cs_DllVer64();

        [DllImport(L32, EntryPoint = "tightdb_c_cs_getver", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tightdb_c_cs_DllVer32();

        private static long CppDllVersion()
        {
            if (Is64Bit)
                return (long) tightdb_c_cs_DllVer64();
            return (long) tightdb_c_cs_DllVer32();
        }



        //TIGHTDB_C_CS_API Table* table_copy_table(tightdb::Table* table_ptr)
        //NOTE!!! The C++ dll uses langbindhelper!
        [DllImport(L64, EntryPoint = "table_copy_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_copy_table64(TableHandle sourceTablePtr);

        [DllImport(L32, EntryPoint = "table_copy_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_copy_table32(TableHandle sourceTablePtr);

        //Called by TableHandle. The returned table is a new root
        public static IntPtr TableCopyTable(TableHandle tableHandle)
        {
            return Is64Bit
                ? table_copy_table64(tableHandle)
                : table_copy_table32(tableHandle);
        }

        [DllImport(L64, EntryPoint = "table_is_attached", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_is_attached64(TableHandle tablePtr);

        [DllImport(L32, EntryPoint = "table_is_attached", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_is_attached32(TableHandle tablePtr);

        public static bool TableIsAttached(Table table)
        {
            return
                IntPtrToBool(Is64Bit ? table_is_attached64(table.TableHandle) : table_is_attached32(table.TableHandle));
        }


        [DllImport(L64, EntryPoint = "table_has_shared_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_has_shared_spec64(TableHandle tablePtr);

        [DllImport(L32, EntryPoint = "table_has_shared_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_has_shared_spec32(TableHandle tablePtr);

        public static bool TableHasSharedSpec(Table table)
        {
            return
                IntPtrToBool(Is64Bit
                    ? table_has_shared_spec64(table.TableHandle)
                    : table_has_shared_spec32(table.TableHandle));
        }



        //todo:verify again that marshalling a criticalhandle actiually fishes out the intptr and sends that one to c++
        //todo:quite sure i read somewhere that CriticalHandle has implemented type conversion to IntPtr        
        [DllImport(L64, EntryPoint = "spec_deallocate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate64(SpecHandle spec);

        [DllImport(L32, EntryPoint = "spec_deallocate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void spec_deallocate32(SpecHandle spec);

        //called by SpecHandle Unbind
        public static void SpecDeallocate(SpecHandle handle)
        {
            if (Is64Bit)
                spec_deallocate64(handle);
            else
                spec_deallocate32(handle);
        }


        //we have to trust that c++ DataType fills up the same amount of stack space as one of our own DataType enum's
        //This is the case on windows, visual studio2010 and 2012 but Who knows if some c++ compiler somewhere someday decides to store DataType differently
        //Marshalling DataType seemed to work very well, but I have chosen to use size_t instead to be 100% sure stack sizes always match. after all, marshalling targets VS2012 on windows specifically
        //furthermore, the cast from IntPtr has been put in a method of its own to ease any fixes to this later on

        private static DataType IntPtrToDataType(IntPtr value)
        {
            return (DataType) value;
        }

        private static IntPtr DataTypeToIntPtr(DataType value)
        {
            return (IntPtr) value;
        }



        private static IntPtr BoolToIntPtr(Boolean value)
        {
            return value ? (IntPtr) 1 : (IntPtr) 0;
        }

        private static Boolean IntPtrToBool(IntPtr value)
        {
            return (IntPtr) 1 == value;
        }

        //gets an IntPtr that is the return value
        //of bool_to_size_t_with_erorrtag or bool_to_size_t
        //from the C++ part of the# binding
        //converts the IntPtr to true or false, as well as returns an errorcode
        //if the return value (errorcode) is 0, boolValue reflects the boolean from c++
        //if the return value is negative, then it is an error code from c++
        //and the value of boolvalaue should be discarded

        //note that if there is an error, the boolValue parameter is forced false (thus unreliable)
        //errorcode is an indication of what kind of error has happened. 
        //Basically, errorcode<0 means an exception was caught in c++
        //Errorcode cannot be greater than 0, it is always negative or 0
        //the reason we stuff two variables into one size_t is that interop overhead is quite large
        //so we reduce the number of parametres to its minimum
        private static long IntPtrToBoolWithErorrCode(IntPtr value, out bool boolValue)
        {
            if ((long) value == 1)
            {
                boolValue = true;
                return 0;
            }
            boolValue = false;
            //in the case value is 0 we must return false. if value is negative it doesn't matter what boolvalue is set to
            //doing it this way removes a branch. Branches are time expensive
            return (long) value; //at this point value is either 0 or a negative error code from c++
        }

        //   value   boolvalue    errorcode
        //     1     true          0
        //     0     false         0
        //    -1     unch.         -1
        //    -2     unch.         -2
        //    -3     unch.         -3
        //    -4     unch.         -4 
        //    -5     unch.         -5

        //usage :  errorcode = IntPtrToBollWithErrorCode(IntPtr retfromcpppcall, ref bool result);
        // if (errorcode<0){throw(some exception depending on errorcode)}
        // return result;


        [DllImport(L32, EntryPoint = "table_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_index32(TableHandle tablehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(L64, EntryPoint = "table_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_index64(TableHandle tablehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        //returns -1 if the column string does not match a column index
        public static long TableGetColumnIndex(Table table, string name)
        {
            return Is64Bit
                ? (long) table_get_column_index64(table.TableHandle, name, (IntPtr) name.Length)
                : (long) table_get_column_index32(table.TableHandle, name, (IntPtr) name.Length);
        }


        [DllImport(L64, EntryPoint = "table_get_subtable_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable_size64(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "table_get_subtable_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable_size32(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex);


        public static long TableGetSubTableSize(Table table, long columnIndex, long rowIndex)
        {
            return
                Is64Bit
                    ? (long)table_get_subtable_size64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                    : (long)table_get_subtable_size32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }



        [DllImport(L32, EntryPoint = "tableview_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_index32(TableViewHandle tableViehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(L64, EntryPoint = "tableview_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_index64(TableViewHandle tableViewhandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        public static long TableViewGetColumnIndex(TableView tableView, string name)
        {
            if (Is64Bit)
                return (long) tableView_get_column_index64(tableView.TableViewHandle, name, (IntPtr) name.Length);
            return (long) tableView_get_column_index32(tableView.TableViewHandle, name, (IntPtr) name.Length);
        }


        [DllImport(L64, EntryPoint = "table_rename_column", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern void table_rename_column64(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(L32, EntryPoint = "table_rename_column", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern void table_rename_column32(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        public static void TableRenameColumn(Table table, long columnIndex, string name)
        {
            if (Is64Bit)
                table_rename_column64(table.TableHandle, (IntPtr)columnIndex, name, (IntPtr)name.Length);
            else
                table_rename_column32(table.TableHandle, (IntPtr)columnIndex, name, (IntPtr)name.Length);
        }




        [DllImport(L64, EntryPoint = "table_remove_column", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern void table_remove_column64(TableHandle tableHandle, long columnIndex);

        [DllImport(L32, EntryPoint = "table_remove_column", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern void table_remove_column32(TableHandle tableHandle, long columnIndex);

        public static void TableRemoveColumn(Table table, long columnIndex)
        {
            if (Is64Bit)
                table_remove_column64(table.TableHandle, columnIndex);
            else
                table_remove_column32(table.TableHandle, columnIndex);
        }





        [DllImport(L64, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_column64(TableHandle tableHandle, IntPtr type,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(L32, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_column32(TableHandle tableHandle, IntPtr type,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        public static long TableAddColumn(Table table, DataType type, string name)
        {

            if (Is64Bit)
                return (long)table_add_column64(table.TableHandle, DataTypeToIntPtr(type), name, (IntPtr)name.Length);
            return (long)table_add_column32(table.TableHandle, DataTypeToIntPtr(type), name, (IntPtr)name.Length);
        }




        [DllImport(L64, EntryPoint = "table_add_subcolumn", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_subcolumn64(TableHandle tableHandle, IntPtr pathLength, IntPtr pathArrayPtr,
            IntPtr type, [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(L32, EntryPoint = "table_add_subcolumn", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_subcolumn32(TableHandle tableHandle, IntPtr pathLength, IntPtr pathArrayPtr,
            IntPtr type, [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        public static long TableAddSubColumn(Table table, IList<long> path, DataType type, string name)
        {
            var pathForCpp = new IntPtr[path.Count];
            var n = 0;
            foreach (var pathIndex in path)
            {
                pathForCpp[n] = (IntPtr) pathIndex;
                ++n;
            }

            GCHandle handle = GCHandle.Alloc(pathForCpp, GCHandleType.Pinned);
            IntPtr valuePointer = handle.AddrOfPinnedObject();

            try
            {
                if (Is64Bit)
                    return
                        (long)
                            table_add_subcolumn64(table.TableHandle, (IntPtr)pathForCpp.Length, valuePointer,
                                DataTypeToIntPtr(type), name, (IntPtr) name.Length);
                return
                    (long)
                        table_add_subcolumn32(table.TableHandle, (IntPtr)pathForCpp.Length, valuePointer,
                            DataTypeToIntPtr(type), name, (IntPtr) name.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        [DllImport(L64, EntryPoint = "table_rename_subcolumn", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_rename_subcolumn64(TableHandle tableHandle, IntPtr pathLength, IntPtr pathArrayPtr,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(L32, EntryPoint = "table_rename_subcolumn", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_rename_subcolumn32(TableHandle tableHandle, IntPtr pathLength, IntPtr pathArrayPtr,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        public static void TableRenameSubColumn(Table table, IList<long> path, string name)
        {
            var pathForCpp = new IntPtr[path.Count];
            var n = 0;
            foreach (var pathIndex in path)
            {
                pathForCpp[n] = (IntPtr) pathIndex;
                n++;
            }

            var handle = GCHandle.Alloc(pathForCpp, GCHandleType.Pinned);
            var valuePointer = handle.AddrOfPinnedObject();

            try
            {
                if (Is64Bit)
                    table_rename_subcolumn64(table.TableHandle, (IntPtr)pathForCpp.Length, valuePointer, name,
                        (IntPtr) name.Length);
                else
                    table_rename_subcolumn32(table.TableHandle, (IntPtr)pathForCpp.Length, valuePointer, name,
                        (IntPtr) name.Length);
            }
            finally
            {
                handle.Free();
            }
        }


        [DllImport(L64, EntryPoint = "table_remove_subcolumn", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_remove_subcolumn64(TableHandle tableHandle, IntPtr pathLength,
            IntPtr pathArrayPtr);

        [DllImport(L32, EntryPoint = "table_remove_subcolumn", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_remove_subcolumn32(TableHandle tableHandle, IntPtr pathLength,
            IntPtr pathArrayPtr);

        public static void TableRemoveSubColumn(Table table, IList<long> path)
        {
            var pathForCpp = new IntPtr[path.Count];
            var n = 0;
            foreach (var pathIndex in path)
            {
                pathForCpp[n] = (IntPtr) pathIndex;
                n++;
            }

            GCHandle handle = GCHandle.Alloc(pathForCpp, GCHandleType.Pinned);
            IntPtr valuePointer = handle.AddrOfPinnedObject();

            try
            {
                if (Is64Bit)
                    table_remove_subcolumn64(table.TableHandle, (IntPtr) pathForCpp.Length, valuePointer);
                else
                    table_remove_subcolumn32(table.TableHandle, (IntPtr) pathForCpp.Length, valuePointer);
            }
            finally
            {
                handle.Free();
            }
        }






        [DllImport(L64, EntryPoint = "spec_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_type64(SpecHandle spechandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "spec_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_type32(SpecHandle spechandle, IntPtr columnIndex);

        public static DataType SpecGetColumnType(Spec s, long columnIndex)
        {
            if (Is64Bit)
                return IntPtrToDataType(spec_get_column_type64(s.SpecHandle, (IntPtr)columnIndex));
            //down here we must be in 32 bit mode (or 16 bit or  more than 64 bit which is NOT supported yet
            //below is not covered by unit tests as we would need a table with quite a lot columns in order to test it
            //or perhaps call in directly somehow
            if (columnIndex > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException("columnIndex",
                    String.Format(CultureInfo.InvariantCulture,
                        "in 32 bit mode, column index must at most be {0} but SpecGetColumnType was called with{1}",
                        Int32.MaxValue, columnIndex));
            }
            return IntPtrToDataType(spec_get_column_type32(s.SpecHandle, (IntPtr)columnIndex));
        }



        //spec colmparison
        [DllImport(L64, EntryPoint = "table_spec_equals_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_spec_equals_spec64(TableHandle tablehandle1, TableHandle tablehandle2);

        [DllImport(L32, EntryPoint = "table_spec_equals_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_spec_equals_spec32(TableHandle tablehandle1, TableHandle tablehandle2);

        public static bool SpecEqualsSpec(Table table1, Table table2)
        {
            return IntPtrToBool(Is64Bit
                ? table_spec_equals_spec64(table1.TableHandle, table2.TableHandle)
                : table_spec_equals_spec32(table1.TableHandle, table2.TableHandle));
        }


        //get a spec given a column index. Returns specs for subtables, but not for mixed (as they would need a row index too)
        //Spec       *spec_get_spec(Spec *spec, size_t column_ndx);
        [DllImport(L64, EntryPoint = "spec_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec64(SpecHandle spec, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "spec_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_spec32(SpecHandle spec, IntPtr columnIndex);

        //used by SpecHandle.GetSubSpecHandle to get an atomic subspec handle created
        public static IntPtr SpecGetSpec(SpecHandle sh, long columnIndex)
        {
            return (Is64Bit)
                ? spec_get_spec64(sh, (IntPtr) columnIndex)
                : spec_get_spec32(sh, (IntPtr) columnIndex);
        }




        //size_t spec_get_column_count(Spec* spec);
        [DllImport(L64, EntryPoint = "spec_get_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_count64(SpecHandle spec);

        [DllImport(L32, EntryPoint = "spec_get_column_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_count32(SpecHandle spec);


        public static long SpecGetColumnCount(Spec spec)
        {
            if (Is64Bit)
                return (long)spec_get_column_count64(spec.SpecHandle);
            //OPTIMIZE  - could be optimized with a 32 or 64 bit specific implementation - see end of file
            return (long)spec_get_column_count32(spec.SpecHandle);
            //OPTIMIZE  - could be optimized with a 32 or 64 bit specific implementation - see end of file
        }


        //tightdb_c_cs_API size_t new_table()
        //The TableHandle will be initialized with root=null and that is exactly what we want
        //when we ask for a freestanding table
        //in the call to new_table we atomically acquire a TableHandle that can finalize itself if need be
        [DllImport(L64, EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern TableHandle new_table64();

        [DllImport(L32, EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern TableHandle new_table32();

        public static void TableNew(Table table, bool isReadOnly)
        {
            table.SetHandle(Is64Bit ? new_table64() : new_table32(), isReadOnly);
        }


        [DllImport(L64, EntryPoint = "group_write", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_write64(GroupHandle groupPtr, [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);

        [DllImport(L32, EntryPoint = "group_write", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_write32(GroupHandle groupPTr, [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);


        //the unmanaged function returns an IntPtr whose value indicates an error if it is nonzero
        //see c++ file for error codes and their meaning
        public static void GroupWrite(Group group, string fileName)
        {
            var res = (Is64Bit)
                ? group_write64(group.GroupHandle, fileName, (IntPtr)fileName.Length)
                : group_write32(group.GroupHandle, fileName, (IntPtr)fileName.Length);

            if (res.ToInt64() == 1)
            {
                throw new IOException("GroupWrite called with an existing file (or other IO error)");
            }
        }


        //a few uncovered lines okay, it is hard to get commit to throw
        [DllImport(L64, EntryPoint = "group_commit", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_commit64(GroupHandle handle);

        [DllImport(L32, EntryPoint = "group_commit", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_commit32(GroupHandle handle);

        public static void GroupCommit(Group group)
        {

            IntPtr res = (Is64Bit)
                ? group_commit64(group.GroupHandle)
                : group_commit32(group.GroupHandle);

            if (res != IntPtr.Zero)
                //shared_group_commit threw an exception in core
                //currently we just assume it was an IO error, but could be anything                
                throw new InvalidOperationException(
                    "Group commit exception in core. probably an IO error with the group file");
        }



        [DllImport(L64, EntryPoint = "group_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr group_to_string64(GroupHandle groupHandle, IntPtr buffer, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "group_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr group_to_string32(GroupHandle groupHandle, IntPtr buffer, IntPtr bufsize);


        public static string GroupToString(Group g)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (int)group_to_string64(g.GroupHandle, buffer, (IntPtr)currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (int)group_to_string32(g.GroupHandle, buffer, (IntPtr)currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }



        [DllImport(L64, EntryPoint = "group_equals", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_equals64(GroupHandle handle1, GroupHandle handle2);

        [DllImport(L32, EntryPoint = "group_equals", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_equals32(GroupHandle handle1, GroupHandle handle2);

        public static Boolean GroupEquals(Group group1, Group group2)
        {
            Boolean result;
            if (IntPtrToBoolWithErorrCode((Is64Bit)
                ? group_equals64(group1.GroupHandle, group2.GroupHandle)
                : group_equals32(group1.GroupHandle, group2.GroupHandle), out result) < 0)
                throw new InvalidOperationException(
                    "Group equals exception in core. probably an IO error with the group file");
            return result; //no errors so just return the result
        }

        //GroupNotEquals can be implemented in group by simply negating the result from GroupEquals




        [DllImport(L64, EntryPoint = "group_is_empty", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_is_empty64(GroupHandle handle);

        [DllImport(L32, EntryPoint = "group_is_empty", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_is_empty32(GroupHandle handle);

        public static Boolean GroupIsEmpty(Group group)
        {
            Boolean result;
            if (IntPtrToBoolWithErorrCode((Is64Bit)
                ? group_is_empty64(group.GroupHandle)
                : group_is_empty32(group.GroupHandle), out result) < 0)
                //shared_group_IsEmpty threw an exception in core
                //currently we just assume it was an IO error, but could be anything
                //more elaborate reporting would need the result from IntPtrToBollWithErrorCode to be saved
                //and used to throw a more precise exception
                throw new InvalidOperationException(
                    "Group commit exception in core. probably an IO error with the group file");
            return result; //no errors so just return the result
        }






        [DllImport(L64, EntryPoint = "group_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_size64(GroupHandle handle);

        [DllImport(L32, EntryPoint = "group_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_size32(GroupHandle handle);

        public static long GroupSize(Group group)
        {
            var result = (long) ((Is64Bit)
                ? group_size64(group.GroupHandle)
                : group_size32(group.GroupHandle));

            if (result < 0)
                //shared_group_IsEmpty threw an exception in core
                //currently we just assume it was an IO error, but could be anything
                //more elaborate reporting would need the result from IntPtrToBollWithErrorCode to be saved
                //and used to throw a more precise exception
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Group size call lead to an exception in core. Group file IO error or group is invalid : {0}", group));
            return result; //no errors so just return the result
        }

















        //as we return a  GroupHandle we get an atomic creation of a group handle where
        //we have a guarentee that if new_group_file64 returned a value, then we will also have a
        //fully constructed GroupHandle with that value. That grouphandle will have root==null, and that
        //is also exactly what we want.
        [DllImport(L64, EntryPoint = "new_group_file", CallingConvention = CallingConvention.Cdecl)]
        private static extern GroupHandle new_group_file64([MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen, IntPtr openMode);

        [DllImport(L32, EntryPoint = "new_group_file", CallingConvention = CallingConvention.Cdecl)]
        private static extern GroupHandle new_group_file32([MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen, IntPtr openMode);

        public static void GroupNewFile(Group group, string fileName, Group.OpenMode openMode)
        {
            IntPtr nativeOpenMode;
            switch (openMode)
            {
                case Group.OpenMode.ModeReadOnly:
                    nativeOpenMode = (IntPtr) 0;
                    break;
                case Group.OpenMode.ModeReadWrite:
                    nativeOpenMode = (IntPtr) 1;
                    break;
                case Group.OpenMode.ModeReadWriteNoCreate:
                    nativeOpenMode = (IntPtr) 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("openMode");
            }

            var handle = Is64Bit
                ? new_group_file64(fileName, (IntPtr) fileName.Length, nativeOpenMode)
                : new_group_file32(fileName, (IntPtr) fileName.Length, nativeOpenMode);


            if (!handle.IsInvalid)
            {
                group.SetHandle(handle, openMode == Group.OpenMode.ModeReadOnly);
            }
            else
            {
                handle.Dispose();//will clean it up, remove it from finalizer queue
                throw new IOException(String.Format(CultureInfo.InvariantCulture,
                    "IO error creating group file {0} (read/write access is needed)", fileName));
            }
        }



        [DllImport(L64, EntryPoint = "new_group", CallingConvention = CallingConvention.Cdecl)]
        private static extern GroupHandle new_group64();

        [DllImport(L32, EntryPoint = "new_group", CallingConvention = CallingConvention.Cdecl)]
        private static extern GroupHandle new_group32();

        //crates a new freestainding group so root is left at null, and ignoreunbind is false
        //this means GroupHandle.unbind is workable as soon as new_group returns, even if sethandle failed
        public static void GroupNew(Group group, Boolean isReadOnly)
        {
            group.SetHandle(Is64Bit
                ? new_group64()
                : new_group32(), isReadOnly);
        }







        [DllImport(L64, EntryPoint = "tableview_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_size64(TableViewHandle tablePtr);

        [DllImport(L32, EntryPoint = "tableview_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_size32(TableViewHandle tablePtr);

        public static long TableViewSize(TableView tv)
        {
            if (Is64Bit)
                return (long)tableview_get_size64(tv.TableViewHandle);
            return (long)tableview_get_size32(tv.TableViewHandle);
        }






        //size_t         find_first_int(size_t column_ndx, int64_t value) const;
        //TIGHTDB_C_CS_API size_t table_find_first_int(Table * table_ptr , size_t column_ndx, int64_t value)

        [DllImport(L64, EntryPoint = "table_find_first_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_int64(TableHandle tableHandle, IntPtr columnIndex, long value);

        [DllImport(L32, EntryPoint = "table_find_first_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_int32(TableHandle tableHandle, IntPtr columnIndex, long value);


        public static long TableFindFirstInt(Table table, long columnIndex, long value)
        {
            return
                Is64Bit
                    ? (long)table_find_first_int64(table.TableHandle, (IntPtr)columnIndex, value)
                    : (long)table_find_first_int32(table.TableHandle, (IntPtr)columnIndex, value);
        }



        [DllImport(L64, EntryPoint = "table_find_first_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_string64(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "table_find_first_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_string32(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);


        public static long TableFindFirstString(Table table, long columnIndex, string value)
        {
            return
                Is64Bit
                    ? (long)table_find_first_string64(table.TableHandle, (IntPtr)columnIndex, value, (IntPtr)value.Length)
                    : (long)table_find_first_string32(table.TableHandle, (IntPtr)columnIndex, value, (IntPtr)value.Length);
        }


        //resharper warning okay, ffb is not implemented in table core
        [DllImport(L64, EntryPoint = "table_find_first_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_binary64(TableHandle tableHandle, IntPtr columnIndex, IntPtr value,
            IntPtr bytes);

        [DllImport(L32, EntryPoint = "table_find_first_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_binary32(TableHandle tableHandle, IntPtr columnIndex, IntPtr value,
            IntPtr bytes);


        //mono does not do well with exceptions thrown from c++ so don't bother calling
#if (__MonoCS__)
        public static long TableFindFirstBinary(Table table, long columnIndex, byte[] value)
        {
                        throw new NotImplementedException("Table Find First has not been implemented in this version ");                
        }

#else
        //not implemented in core c++ dll will return -1 until it is implemented
        public static long TableFindFirstBinary(Table table, long columnIndex, byte[] value)
        {
            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            try
            {
                var valuePointer = handle.AddrOfPinnedObject();
                return
                    Is64Bit
                        ? (long)
                            table_find_first_binary64(table.TableHandle, (IntPtr) columnIndex, valuePointer,
                                (IntPtr) value.Length)
                        : (long)
                            table_find_first_binary32(table.TableHandle, (IntPtr) columnIndex, valuePointer,
                                (IntPtr) value.Length);
            }
            finally
            {
                handle.Free();
            }
        }

#endif




        [DllImport(L64, EntryPoint = "table_find_first_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_double64(TableHandle tableHandle, IntPtr columnIndex, double value);

        [DllImport(L32, EntryPoint = "table_find_first_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_double32(TableHandle tableHandle, IntPtr columnIndex, double value);


        public static long TableFindFirstDouble(Table table, long columnIndex, double value)
        {
            return
                Is64Bit
                    ? (long) table_find_first_double64(table.TableHandle, (IntPtr) columnIndex, value)
                    : (long) table_find_first_double32(table.TableHandle, (IntPtr) columnIndex, value);
        }



        [DllImport(L64, EntryPoint = "table_find_first_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_float64(TableHandle tableHandle, IntPtr columnIndex, float value);

        [DllImport(L32, EntryPoint = "table_find_first_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_float32(TableHandle tableHandle, IntPtr columnIndex, float value);


        public static long TableFindFirstFloat(Table table, long columnIndex, float value)
        {
            return
                Is64Bit
                    ? (long) table_find_first_float64(table.TableHandle, (IntPtr) columnIndex, value)
                    : (long) table_find_first_float32(table.TableHandle, (IntPtr) columnIndex, value);
        }


        [DllImport(L64, EntryPoint = "table_find_first_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_date64(TableHandle tableHandle, IntPtr columnIndex, Int64 value);

        [DllImport(L32, EntryPoint = "table_find_first_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_date32(TableHandle tableHandle, IntPtr columnIndex, Int64 value);


        public static long TableFindFirstDate(Table table, long columnIndex, DateTime value)
        {
            return
                Is64Bit
                    ? (long) table_find_first_date64(table.TableHandle, (IntPtr) columnIndex, ToTightDbDateTime(value))
                    : (long) table_find_first_date32(table.TableHandle, (IntPtr) columnIndex, ToTightDbDateTime(value));
        }



        [DllImport(L64, EntryPoint = "table_find_first_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_bool64(TableHandle tableHandle, IntPtr columnIndex, IntPtr value);

        [DllImport(L32, EntryPoint = "table_find_first_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_first_bool32(TableHandle tableHandle, IntPtr columnIndex, IntPtr value);


        public static long TableFindFirstBool(Table table, long columnIndex, bool value)
        {
            return
                Is64Bit
                    ? (long) table_find_first_bool64(table.TableHandle, (IntPtr) columnIndex, BoolToIntPtr(value))
                    : (long) table_find_first_bool32(table.TableHandle, (IntPtr) columnIndex, BoolToIntPtr(value));
        }





        [DllImport(L64, EntryPoint = "tableview_get_subtable_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_subtable_size64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "tableview_get_subtable_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_subtable_size32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            IntPtr rowIndex);


        public static long TableViewGetSubTableSize(TableView tableView, long columnIndex, long rowIndex)
        {
            return
                Is64Bit
                    ? (long)
                        tableView_get_subtable_size64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                    : (long)
                        tableView_get_subtable_size32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }






        //find first in tableview
        //size_t         find_first_int(size_t column_ndx, int64_t value) const;
        //TIGHTDB_C_CS_API size_t table_find_first_int(Table * table_ptr , size_t column_ndx, int64_t value)

        [DllImport(L64, EntryPoint = "tableview_find_first_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_int64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            long value);

        [DllImport(L32, EntryPoint = "tableview_find_first_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_int32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            long value);


        public static long TableViewFindFirstInt(TableView tableView, long columnIndex, long value)
        {
            return
                Is64Bit
                    ? (long) tableView_find_first_int64(tableView.TableViewHandle, (IntPtr) columnIndex, value)
                    : (long) tableView_find_first_int32(tableView.TableViewHandle, (IntPtr) columnIndex, value);
        }



        [DllImport(L64, EntryPoint = "tableview_find_first_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_string64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "tableview_find_first_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_string32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);


        public static long TableViewFindFirstString(TableView tableView, long columnIndex, string value)
        {
            return
                Is64Bit
                    ? (long)
                        tableView_find_first_string64(tableView.TableViewHandle, (IntPtr) columnIndex, value,
                            (IntPtr) value.Length)
                    : (long)
                        tableView_find_first_string32(tableView.TableViewHandle, (IntPtr) columnIndex, value,
                            (IntPtr) value.Length);
        }


        //todo:verify that the marshalling used below really works, if it does, we can simplify some other calles that move binaries
        //over to core

        [DllImport(L64, EntryPoint = "tableview_find_first_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_binary64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [In] byte[] value, IntPtr length);

        [DllImport(L32, EntryPoint = "tableview_find_first_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_binary32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [In] byte[] value, IntPtr length);


        public static long TableViewFindFirstBinary(TableView tableView, long columnIndex, byte[] value)
        {
//            if (IsRunningOnMono())
//            {
//                throw new System.NotImplementedException("Table.FindFirstBinary has not been implemented in core");
//            }

            return
                Is64Bit
                    ? (long)
                        tableView_find_first_binary64(tableView.TableViewHandle, (IntPtr) columnIndex, value,
                            (IntPtr) value.Length)
                    : (long)
                        tableView_find_first_binary32(tableView.TableViewHandle, (IntPtr) columnIndex, value,
                            (IntPtr) value.Length);
        }






        [DllImport(L64, EntryPoint = "tableview_find_first_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_double64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            double value);

        [DllImport(L32, EntryPoint = "tableview_find_first_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_double32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            double value);


        public static long TableViewFindFirstDouble(TableView tableView, long columnIndex, double value)
        {
            return
                Is64Bit
                    ? (long) tableView_find_first_double64(tableView.TableViewHandle, (IntPtr) columnIndex, value)
                    : (long)tableView_find_first_double32(tableView.TableViewHandle, (IntPtr)columnIndex, value);
        }



        [DllImport(L64, EntryPoint = "tableview_find_first_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_float64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            float value);

        [DllImport(L32, EntryPoint = "tableview_find_first_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_float32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            float value);


        public static long TableViewFindFirstFloat(TableView tableView, long columnIndex, float value)
        {
            return
                Is64Bit
                    ? (long)tableView_find_first_float64(tableView.TableViewHandle, (IntPtr)columnIndex, value)
                    : (long)tableView_find_first_float32(tableView.TableViewHandle, (IntPtr)columnIndex, value);
        }



        [DllImport(L64, EntryPoint = "tableview_find_first_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_date64(TableViewHandle tableViewHandle, IntPtr columnIndex, Int64 value);

        [DllImport(L32, EntryPoint = "tableview_find_first_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_date32(TableViewHandle tableViewHandle, IntPtr columnIndex, Int64 value);


        public static long TableViewFindFirstDate(TableView tableView, long columnIndex, DateTime value)
        {
            return
                Is64Bit
                    ? (long)
                        tableView_find_first_date64(tableView.TableViewHandle, (IntPtr)columnIndex, ToTightDbDateTime(value))
                    : (long)
                        tableView_find_first_date32(tableView.TableViewHandle, (IntPtr)columnIndex, ToTightDbDateTime(value));
        }



        [DllImport(L64, EntryPoint = "tableview_find_first_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_bool64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            IntPtr value);

        [DllImport(L32, EntryPoint = "tableview_find_first_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_first_bool32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            IntPtr value);


        public static long TableViewFindFirstBool(TableView tableView, long columnIndex, bool value)
        {
            return
                Is64Bit
                    ? (long)tableView_find_first_bool64(tableView.TableViewHandle, (IntPtr)columnIndex, BoolToIntPtr(value))
                    : (long)tableView_find_first_bool32(tableView.TableViewHandle, (IntPtr)columnIndex, BoolToIntPtr(value));
        }





        [DllImport(L64, EntryPoint = "table_distinct", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_distinct64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_distinct", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_distinct32(TableHandle tableHandle, IntPtr columnIndex);

        //called by TableHandle to create a tableView atomically using a TableDistinct call
        public static IntPtr TableDistinct(TableHandle tableHandle, long columnIndex)
        {
            return (Is64Bit)
                ? table_distinct64(tableHandle, (IntPtr) columnIndex)
                : table_distinct32(tableHandle, (IntPtr) columnIndex);
        }




        [DllImport(L64, EntryPoint = "table_count_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_int64(TableHandle tableHandle, IntPtr columnIndex, long target);

        [DllImport(L32, EntryPoint = "table_count_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_int32(TableHandle tableHandle, IntPtr columnIndex, long target);


        public static long TableCountLong(Table table, long columnIndex, long target)
        {
            return Is64Bit
                ? table_count_int64(table.TableHandle, (IntPtr) columnIndex, target)
                : table_count_int32(table.TableHandle, (IntPtr) columnIndex, target);
        }

        [DllImport(L64, EntryPoint = "table_count_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_float64(TableHandle tableHandle, IntPtr columnIndex, float target);

        [DllImport(L32, EntryPoint = "table_count_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_float32(TableHandle tableHandle, IntPtr columnIndex, float target);


        public static long TableCountFloat(Table table, long columnIndex, float target)
        {
            return Is64Bit
                ? table_count_float64(table.TableHandle, (IntPtr) columnIndex, target)
                : table_count_float32(table.TableHandle, (IntPtr) columnIndex, target);
        }

        [DllImport(L64, EntryPoint = "table_count_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_double64(TableHandle tableHandle, IntPtr columnIndex, double target);

        [DllImport(L32, EntryPoint = "table_count_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_double32(TableHandle tableHandle, IntPtr columnIndex, double target);


        public static long TableCountDouble(Table table, long columnIndex, double target)
        {
            return Is64Bit
                ? table_count_double64(table.TableHandle, (IntPtr) columnIndex, target)
                : table_count_double32(table.TableHandle, (IntPtr) columnIndex, target);
        }


        [DllImport(L64, EntryPoint = "table_count_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_string64(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string target, IntPtr targetLen);

        [DllImport(L32, EntryPoint = "table_count_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_count_string32(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string target, IntPtr targetLen);


        public static long TableCountString(Table table, long columnIndex, string target)
        {
            return Is64Bit
                ? table_count_string64(table.TableHandle, (IntPtr) columnIndex, target, (IntPtr) target.Length)
                : table_count_string32(table.TableHandle, (IntPtr) columnIndex, target, (IntPtr) target.Length);
        }


        [DllImport(L64, EntryPoint = "table_sum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_sum_int64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_sum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_sum_int32(TableHandle tableHandle, IntPtr columnIndex);


        public static long TableSumLong(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_sum_int64(table.TableHandle, (IntPtr) columnIndex)
                : table_sum_int32(table.TableHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "table_sum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_sum_float64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_sum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_sum_float32(TableHandle tableHandle, IntPtr columnIndex);



        public static double TableSumFloat(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_sum_float64(table.TableHandle, (IntPtr) columnIndex)
                : table_sum_float32(table.TableHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "table_sum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_sum_double64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_sum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_sum_double32(TableHandle tableHandle, IntPtr columnIndex);


        public static double TableSumDouble(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_sum_double64(table.TableHandle, (IntPtr) columnIndex)
                : table_sum_double32(table.TableHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_maximum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_maximum64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_maximum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_maximum32(TableHandle tableHandle, IntPtr columnIndex);


        public static long TableMaximumLong(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_maximum64(table.TableHandle, (IntPtr) columnIndex)
                : table_maximum32(table.TableHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "table_maximum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_maximum_float64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_maximum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_maximum_float32(TableHandle tableHandle, IntPtr columnIndex);


        public static float TableMaximumFloat(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_maximum_float64(table.TableHandle, (IntPtr) columnIndex)
                : table_maximum_float32(table.TableHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "table_maximum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_maximum_double64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_maximum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_maximum_double32(TableHandle tableHandle, IntPtr columnIndex);


        public static double TableMaximumDouble(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_maximum_double64(table.TableHandle, (IntPtr) columnIndex)
                : table_maximum_double32(table.TableHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_maximum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_maximum_datetime64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_maximum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_maximum_datetime32(TableHandle tableHandle, IntPtr columnIndex);


        public static DateTime TableMaximumDateTime(Table table, long columnIndex)
        {
            return ToCSharpTimeUtc(Is64Bit
                ? table_maximum_datetime64(table.TableHandle, (IntPtr) columnIndex)
                : table_maximum_datetime32(table.TableHandle, (IntPtr) columnIndex));
        }


        [DllImport(L64, EntryPoint = "table_minimum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_minimum64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_minimum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_minimum32(TableHandle tableHandle, IntPtr columnIndex);


        public static long TableMinimum(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_minimum64(table.TableHandle, (IntPtr) columnIndex)
                : table_minimum32(table.TableHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_minimum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_minimum_float64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_minimum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_minimum_float32(TableHandle tableHandle, IntPtr columnIndex);


        public static float TableMinimumFloat(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_minimum_float64(table.TableHandle, (IntPtr) columnIndex)
                : table_minimum_float32(table.TableHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_minimum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_minimum_double64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_minimum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_minimum_double32(TableHandle tableHandle, IntPtr columnIndex);


        public static double TableMinimumDouble(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_minimum_double64(table.TableHandle, (IntPtr) columnIndex)
                : table_minimum_double32(table.TableHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_minimum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_minimum_datetime64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_minimum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_minimum_datetime32(TableHandle tableHandle, IntPtr columnIndex);


        public static DateTime TableMinimumDateTime(Table table, long columnIndex)
        {
            return ToCSharpTimeUtc(Is64Bit
                ? table_minimum_datetime64(table.TableHandle, (IntPtr) columnIndex)
                : table_minimum_datetime32(table.TableHandle, (IntPtr) columnIndex));
        }


        [DllImport(L64, EntryPoint = "table_average_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_average64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_average_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_average32(TableHandle tableHandle, IntPtr columnIndex);


        public static double TableAverage(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_average64(table.TableHandle, (IntPtr) columnIndex)
                : table_average32(table.TableHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_average_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_average_float64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_average_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_average_float32(TableHandle tableHandle, IntPtr columnIndex);


        public static double TableAverageFloat(Table table, long columnIndex)
        {
            return Is64Bit
                ? table_average_float64(table.TableHandle, (IntPtr) columnIndex)
                : table_average_float32(table.TableHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_average_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_average_double64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_average_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double table_average_double32(TableHandle tableHandle, IntPtr columnIndex);


        public static double TableAverageDouble(Table table, long columnIndex)
        {
            return Is64Bit ? 
                table_average_double64(table.TableHandle, (IntPtr)columnIndex) : table_average_double32(table.TableHandle, (IntPtr)columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_maximum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_maximum64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_maximum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_maximum32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static long TableViewMaximum(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_maximum64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_maximum32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "tableview_maximum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableview_maximum_float64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_maximum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableview_maximum_float32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static float TableViewMaximumFloat(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_maximum_float64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_maximum_float32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "tableview_maximum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_maximum_double64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_maximum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_maximum_double32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static double TableViewMaximumDouble(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_maximum_double64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_maximum_double32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_maximum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_maximum_datetime64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_maximum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_maximum_datetime32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static DateTime TableViewMaximumDateTime(TableView tableView, long columnIndex)
        {
            return ToCSharpTimeUtc(Is64Bit
                ? tableview_maximum_datetime64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_maximum_datetime32(tableView.TableViewHandle, (IntPtr) columnIndex));
        }


        [DllImport(L64, EntryPoint = "tableview_minimum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_minimum64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_minimum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_minimum32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static long TableViewMinimumLong(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_minimum64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_minimum32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_minimum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableview_minimum_float64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_minimum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableview_minimum_float32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static float TableViewMinimumFloat(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_minimum_float64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_minimum_float32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_minimum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_minimum_double64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_minimum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_minimum_double32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static double TableViewMinimumDouble(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_minimum_double64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_minimum_double32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_minimum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_minimum_datetime64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_minimum_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_minimum_datetime32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static DateTime TableViewMinimumDateTime(TableView tableView, long columnIndex)
        {
            return
                ToCSharpTimeUtc(Is64Bit
                    ? tableview_minimum_datetime64(tableView.TableViewHandle, (IntPtr) columnIndex)
                    : tableview_minimum_datetime32(tableView.TableViewHandle, (IntPtr) columnIndex));
        }


        [DllImport(L64, EntryPoint = "tableview_sort_default", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_sort_default64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_sort_default", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_sort_default32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static void TableViewSort(TableView tableView, long columnIndex)
        {
            if (Is64Bit)
                tableview_sort_default64(tableView.TableViewHandle, (IntPtr) columnIndex);
            else
                tableview_sort_default32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "tableview_sort", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_sort64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            IntPtr ascending);

        [DllImport(L32, EntryPoint = "tableview_sort", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_sort32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            IntPtr ascending);


        public static void TableViewSort(TableView tableView, long columnIndex, Boolean ascending)
        {
            if (Is64Bit)
                tableview_sort64(tableView.TableViewHandle, (IntPtr) columnIndex, BoolToIntPtr(ascending));
            else
                tableview_sort32(tableView.TableViewHandle, (IntPtr) columnIndex, BoolToIntPtr(ascending));
        }


        [DllImport(L64, EntryPoint = "tableview_average_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_average64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_average_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_average32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static double TableViewAverageLong(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_average64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_average32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_average_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_average_float64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_average_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_average_float32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static double TableViewAverageFloat(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_average_float64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_average_float32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_average_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_average_double64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_average_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_average_double32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static double TableViewAverageDouble(TableView tableview, long columnIndex)
        {
            return Is64Bit
                ? tableview_average_double64(tableview.TableViewHandle, (IntPtr) columnIndex)
                : tableview_average_double32(tableview.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_sum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_sum64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_sum_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_sum32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static long TableViewSumLong(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_sum64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_sum32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "tableview_sum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_sum_float64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_sum_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_sum_float32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static double TableViewSumFloat(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_sum_float64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_sum_float32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }

        [DllImport(L64, EntryPoint = "tableview_sum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_sum_double64(TableViewHandle tableViewHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_sum_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double tableview_sum_double32(TableViewHandle tableViewHandle, IntPtr columnIndex);


        public static double TableViewSumDouble(TableView tableView, long columnIndex)
        {
            return Is64Bit
                ? tableview_sum_double64(tableView.TableViewHandle, (IntPtr) columnIndex)
                : tableview_sum_double32(tableView.TableViewHandle, (IntPtr) columnIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_count_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_int64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            long target);

        [DllImport(L32, EntryPoint = "tableview_count_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_int32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            long target);


        public static long TableViewCountLong(TableView tableView, long columnIndex, long target)
        {
            return Is64Bit
                ? tableview_count_int64(tableView.TableViewHandle, (IntPtr) columnIndex, target)
                : tableview_count_int32(tableView.TableViewHandle, (IntPtr) columnIndex, target);
        }

        [DllImport(L64, EntryPoint = "tableview_count_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_float64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            float target);

        [DllImport(L32, EntryPoint = "tableview_count_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_float32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            float target);


        public static long TableViewCountFloat(TableView tableView, long columnIndex, float target)
        {
            return Is64Bit
                ? tableview_count_float64(tableView.TableViewHandle, (IntPtr) columnIndex, target)
                : tableview_count_float32(tableView.TableViewHandle, (IntPtr) columnIndex, target);
        }

        [DllImport(L64, EntryPoint = "tableview_count_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_double64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            double target);

        [DllImport(L32, EntryPoint = "tableview_count_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_double32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            double target);


        public static long TableViewCountDouble(TableView tableView, long columnIndex, double target)
        {
            return Is64Bit
                ? tableview_count_double64(tableView.TableViewHandle, (IntPtr) columnIndex, target)
                : tableview_count_double32(tableView.TableViewHandle, (IntPtr) columnIndex, target);
        }


        [DllImport(L64, EntryPoint = "tableview_count_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_string64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string target, IntPtr targetLen);

        [DllImport(L32, EntryPoint = "tableview_count_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableview_count_string32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string target, IntPtr targetLen);


        public static long TableViewCountString(TableView tableView, long columnIndex, string target)
        {
            return Is64Bit
                ? tableview_count_string64(tableView.TableViewHandle, (IntPtr) columnIndex, target,
                    (IntPtr) target.Length)
                : tableview_count_string32(tableView.TableViewHandle, (IntPtr) columnIndex, target,
                    (IntPtr) target.Length);
        }


        [DllImport(L64, EntryPoint = "table_set_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_index64(TableHandle tableHandle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_set_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_index32(TableHandle tableHandle, IntPtr columnIndex);


        public static void TableSetIndex(Table table, long columnIndex)
        {
            if (Is64Bit)
                table_set_index64(table.TableHandle, (IntPtr) columnIndex);
            else
                table_set_index32(table.TableHandle, (IntPtr) columnIndex);
        }





        [DllImport(L64, EntryPoint = "table_find_all_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_int64(TableHandle tableHandle, IntPtr columnIndex, long value);

        [DllImport(L32, EntryPoint = "table_find_all_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_int32(TableHandle tableHandle, IntPtr columnIndex, long value);

        //called by TableHandle atomically
        public static IntPtr TableFindAllInt(TableHandle tableHandle, long columnIndex, long value)
        {
            return Is64Bit
                ? table_find_all_int64(tableHandle, (IntPtr) columnIndex, value)
                : table_find_all_int32(tableHandle, (IntPtr) columnIndex, value);
        }



        [DllImport(L64, EntryPoint = "table_find_all_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_bool64(TableHandle tableHandle, IntPtr columnIndex, IntPtr value);

        [DllImport(L32, EntryPoint = "table_find_all_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_bool32(TableHandle tableHandle, IntPtr columnIndex, IntPtr value);

        //called by TableHande atomically
        public static IntPtr TableFindAllBool(TableHandle tableHandle, long columnIndex, bool value)
        {
            return Is64Bit
                ? table_find_all_bool64(tableHandle, (IntPtr) columnIndex, BoolToIntPtr(value))
                : table_find_all_bool32(tableHandle, (IntPtr) columnIndex, BoolToIntPtr(value));
        }



        [DllImport(L64, EntryPoint = "table_find_all_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_datetime64(TableHandle tableHandle, IntPtr columnIndex, Int64 value);

        [DllImport(L32, EntryPoint = "table_find_all_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_datetime32(TableHandle tableHandle, IntPtr columnIndex, Int64 value);


        public static IntPtr TableFindAllDateTime(TableHandle tableHandle, long columnIndex, DateTime value)
            //todo: consider if all these calls could become internal. Only called by handles anyway. 
        {
            return Is64Bit
                ? table_find_all_datetime64(tableHandle, (IntPtr) columnIndex, ToTightDbDateTime(value))
                : table_find_all_datetime32(tableHandle, (IntPtr) columnIndex, ToTightDbDateTime(value));
        }





        [DllImport(L64, EntryPoint = "table_find_all_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_float64(TableHandle tableHandle, IntPtr columnIndex, float value);

        [DllImport(L32, EntryPoint = "table_find_all_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_float32(TableHandle tableHandle, IntPtr columnIndex, float value);

        //called by TableHandle atomically
        public static IntPtr TableFindAllFloat(TableHandle tableHandle, long columnIndex, float value)
        {
            return Is64Bit
                ? table_find_all_float64(tableHandle, (IntPtr) columnIndex, value)
                : table_find_all_float32(tableHandle, (IntPtr) columnIndex, value);
        }




        [DllImport(L64, EntryPoint = "table_find_all_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_double64(TableHandle tableHandle, IntPtr columnIndex, double value);

        [DllImport(L32, EntryPoint = "table_find_all_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_double32(TableHandle tableHandle, IntPtr columnIndex, double value);

        //Called by TableHandle atomically
        public static IntPtr TableFindAllDouble(TableHandle tableHandle, long columnIndex, double value)
        {
            return Is64Bit
                ? table_find_all_double64(tableHandle, (IntPtr) columnIndex, value)
                : table_find_all_double32(tableHandle, (IntPtr) columnIndex, value);
        }


        //not using automatic marshalling (which might lead to copying in some cases),
        //but ensuring no copying of the array data is done, by getting a pinned pointer to the array supplied by the user.
        //todo: unit test that find all binary with a null pointer actually works okay
        //todo:unit test that find all binary with an empty byte array works okay
        [DllImport(L64, EntryPoint = "table_find_all_empty_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_empty_binary64(TableHandle tableHandle, IntPtr columnNdx);

        [DllImport(L32, EntryPoint = "table_find_all_empty_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_empty_binary32(TableHandle tableHandle, IntPtr columnNdx);


        [DllImport(L64, EntryPoint = "table_find_all_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_binary64(TableHandle tableHandle, IntPtr columnNdx, IntPtr value,
            IntPtr bytes);

        [DllImport(L32, EntryPoint = "table_find_all_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_binary32(TableHandle tableHandle, IntPtr columnNdx, IntPtr value,
            IntPtr bytes);



        //called by tablehandle atomically
        public static IntPtr TableFindAllEmptyBinary(TableHandle tableHandle, long columnIndex)
        {
            return Is64Bit
                ? table_find_all_empty_binary64(tableHandle, (IntPtr) columnIndex)
                : table_find_all_empty_binary32(tableHandle, (IntPtr) columnIndex);
        }

        //called by tablehandle atomically
        public static IntPtr TableFindAllBinary(TableHandle tableHandle, long columnIndex, IntPtr binaryToFind,
            IntPtr size)
        {
            return Is64Bit
                ? table_find_all_binary64(tableHandle, (IntPtr) columnIndex, binaryToFind, size)
                : table_find_all_binary32(tableHandle, (IntPtr) columnIndex, binaryToFind, size);
        }


        //TIGHTDB_C_CS_API tightdb::TableView* tableView_find_all_int(Table * table_ptr , size_t column_ndx, int64_t value)
        [DllImport(L64, EntryPoint = "tableview_find_all_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_int64(TableViewHandle tableViewHandle, IntPtr columnIndex, long value);

        [DllImport(L32, EntryPoint = "tableview_find_all_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_int32(TableViewHandle tableViewHandle, IntPtr columnIndex, long value);


        //called by tableViewHandle to set up root correctly
        internal static IntPtr TableViewFindAllInt(TableViewHandle tableViewHandle, long columnIndex, long value)
        {
            return Is64Bit
                ? tableView_find_all_int64(tableViewHandle,(IntPtr) columnIndex, value)
                : tableView_find_all_int32(tableViewHandle, (IntPtr)columnIndex, value);
        }


        [DllImport(L64, EntryPoint = "tableview_find_all_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_bool64(TableViewHandle tableViewHandle, IntPtr columnIndex, IntPtr value);

        [DllImport(L32, EntryPoint = "tableview_find_all_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_bool32(TableViewHandle tableViewHandle, IntPtr columnIndex, IntPtr value);


        //called from TableViewHandle atomically, we don't want to use TableViewHandle return type as that object would
        //then finalize wrongly, not having the corret root set up. CER makes sure we are not stopped by out-of-band exceptions
        public static IntPtr TableViewFindAllBool(TableViewHandle tableViewHandle, long columnIndex, bool value)
        {
            return Is64Bit
                ? tableView_find_all_bool64(tableViewHandle, (IntPtr)columnIndex, BoolToIntPtr(value))
                : tableView_find_all_bool32(tableViewHandle, (IntPtr)columnIndex, BoolToIntPtr(value));
        }


        //todo what if find all datetime is called on a mixed datetime field. mixed datetime fileds does not like negative time_t values
        //todo do some unit tests on this - both table and tableview
        [DllImport(L64, EntryPoint = "tableview_find_all_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_datetime64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            Int64 value);

        [DllImport(L32, EntryPoint = "tableview_find_all_datetime", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_datetime32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            Int64 value);


        internal static IntPtr TableViewFindAllDateTime(TableViewHandle tableViewHandle, long columnIndex,
            DateTime value)
        {
            return Is64Bit
                ? tableView_find_all_datetime64(tableViewHandle, (IntPtr)columnIndex, ToTightDbDateTime(value))
                : tableView_find_all_datetime32(tableViewHandle, (IntPtr)columnIndex, ToTightDbDateTime(value));
        }


        [DllImport(L64, EntryPoint = "tableview_find_all_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_float64(TableViewHandle tableViewHandle, IntPtr columnIndex, float value);

        [DllImport(L32, EntryPoint = "tableview_find_all_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_float32(TableViewHandle tableViewHandle, IntPtr columnIndex, float value);

        //called by TableViewHandle to create a new TableViewHandle
        internal static IntPtr TableViewFindAllFloat(TableViewHandle tableViewHandle, long columnIndex, float value)
        {
            return Is64Bit
                ? tableView_find_all_float64(tableViewHandle, (IntPtr) columnIndex, value)
                : tableView_find_all_float32(tableViewHandle, (IntPtr) columnIndex, value);
        }


        [DllImport(L64, EntryPoint = "tableview_find_all_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_double64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            double value);

        [DllImport(L32, EntryPoint = "tableview_find_all_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_double32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            double value);

        //called by TableViewHandle to create a new TableViewHandle
        public static IntPtr TableViewFindAllDouble(TableViewHandle tableViewHandle, long columnIndex, double value)
        {
            return Is64Bit
                ? tableView_find_all_double64(tableViewHandle, (IntPtr)columnIndex, value)
                : tableView_find_all_double32(tableViewHandle, (IntPtr)columnIndex, value);
        }


        [DllImport(L64, EntryPoint = "table_find_all_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_string64(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "table_find_all_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_find_all_string32(TableHandle tableHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        //called by TableHandle
        public static IntPtr TableFindAllString(TableHandle tableHandle, long columnIndex, string value)
        {
            return Is64Bit
                ? table_find_all_string64(tableHandle, (IntPtr)columnIndex, value, (IntPtr) value.Length)
                : table_find_all_string32(tableHandle, (IntPtr)columnIndex, value, (IntPtr)value.Length);
        }


        //TIGHTDB_C_CS_API tightdb::TableView* table_find_all_int(Table * table_ptr , size_t column_ndx, int64_t value)
        [DllImport(L64, EntryPoint = "tableview_find_all_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_string64(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "tableview_find_all_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_find_all_string32(TableViewHandle tableViewHandle, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);


        public static IntPtr TableViewFindAllString(TableViewHandle tableViewHandle, long columnIndex, string value)
        {
            return Is64Bit
                ? tableView_find_all_string64(tableViewHandle, (IntPtr)columnIndex,value, (IntPtr)value.Length)
                : tableView_find_all_string32(tableViewHandle, (IntPtr)columnIndex,value, (IntPtr)value.Length);
        }


        //        TIGHTDB_C_CS_API tightdb::TableView* query_find_all_int(Query * query_ptr , size_t start, size_t end, size_t limit)
        [DllImport(L64, EntryPoint = "query_find_all", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find_all64(QueryHandle handle, IntPtr start, IntPtr end, IntPtr limit);

        [DllImport(L32, EntryPoint = "query_find_all", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find_all32(QueryHandle handle, IntPtr start, IntPtr end, IntPtr limit);

        //called by QueryHandle
        internal static IntPtr QueryFindAll(QueryHandle queryHandle, long start, long end, long limit)
        {
            return Is64Bit
                ? query_find_all64(queryHandle, (IntPtr) start, (IntPtr) end, (IntPtr) limit)
                : query_find_all32(queryHandle, (IntPtr) start, (IntPtr) end, (IntPtr) limit);
        }

        //        TIGHTDB_C_CS_API tightdb::TableView* query_find_all_int(Query * query_ptr , size_t start, size_t end, size_t limit)
        [DllImport(L64, EntryPoint = "query_find_all_np", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find_all_np64(QueryHandle handle);

        [DllImport(L32, EntryPoint = "query_find_all_np", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find_all_np32(QueryHandle handle);

        internal static IntPtr QueryFindAll(QueryHandle queryHandle)
        {
            return Is64Bit
                ? query_find_all_np64(queryHandle)
                : query_find_all_np32(queryHandle);
        }


        [DllImport(L64, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find64(QueryHandle queryHandle, IntPtr lastMatch);

        [DllImport(L32, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find32(QueryHandle queryHandle, IntPtr lastMatch);

        public static long QueryFind(Query query, long lastMatch)
        {
            return
                Is64Bit
                    ? (long) query_find64(query.QueryHandle, (IntPtr) lastMatch)
                    : (long) query_find32(query.QueryHandle, (IntPtr) lastMatch);
        }

        [DllImport(L64, EntryPoint = "query_average", CallingConvention = CallingConvention.Cdecl)]
        private static extern double query_average64(QueryHandle handle, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "query_average", CallingConvention = CallingConvention.Cdecl)]
        private static extern double query_average32(QueryHandle handle, IntPtr columnIndex);


        public static double QueryAverage(Query query, long columnIndex)
        {
            return
                Is64Bit
                    ? query_average64(query.QueryHandle, (IntPtr)columnIndex)
                    : query_average32(query.QueryHandle, (IntPtr)columnIndex);
        }


        [DllImport(L64, EntryPoint = "table_clear_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_clear_subtable64(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "table_clear_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_clear_subtable32(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        public static void TableClearSubTable(Table parentTable, long columnIndex, long rowIndex)
        {
            if (Is64Bit)
                table_clear_subtable64(parentTable.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            else
                table_clear_subtable32(parentTable.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_clear_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_clear_subtable64(TableViewHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "tableview_clear_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_clear_subtable32(TableViewHandle  tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        public static void TableViewClearSubTable(TableView parentTableView, long columnIndex, long rowIndex)
        {
            if (Is64Bit)
                tableview_clear_subtable64(parentTableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            else
                tableview_clear_subtable32(parentTableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }




        //this one work on standard subtable columns as well as on mixed subtable columns.
        [DllImport(L64, EntryPoint = "table_get_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable64(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "table_get_subtable", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_subtable32(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex);

        //called by tableHandle
        internal static IntPtr TableGetSubTable(TableHandle tableHandle, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? table_get_subtable64(tableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : table_get_subtable32(tableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }


        //If the name exists in the group, the table associated with the name is returned
        //if the name does not exist in the group, a new table is created and returned
        [DllImport(L64, EntryPoint = "group_get_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_get_table64(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);

        [DllImport(L32, EntryPoint = "group_get_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_get_table32(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);


        //used by GroupHandle to get a table handle atomically with the group handle as root
        //therefore returns intptr
        public static IntPtr GroupGetTable(GroupHandle groupHandle, string tableName)
        {
            return (Is64Bit)
                ? group_get_table64(groupHandle, tableName, (IntPtr) tableName.Length)
                : group_get_table32(groupHandle, tableName, (IntPtr) tableName.Length);
        }



        //Return the table at the specified index in the group.
        //the index relates to the sequence of tables returned with foreach
        //we rely on the table constructor to throw if we get a  null from c+
        [DllImport(L64, EntryPoint = "group_get_table_by_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_get_table_by_index64(GroupHandle groupHandle, IntPtr tableIndex);

        [DllImport(L32, EntryPoint = "group_get_table_by_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_get_table_by_index32(GroupHandle groupHandle, IntPtr tableIndex);

        //called by GroupHandle
        public static IntPtr GroupGetTable(GroupHandle groupHandle, long tableIndex)
        {
            return Is64Bit
                ? group_get_table_by_index64(groupHandle, (IntPtr) tableIndex)
                : group_get_table_by_index32(groupHandle, (IntPtr) tableIndex);
        }





        //
        [DllImport(L64, EntryPoint = "group_has_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_has_table64(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);

        [DllImport(L32, EntryPoint = "group_has_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_has_table32(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);

        public static bool GroupHasTable(Group group, string tableName)
        {
            return IntPtrToBool(Is64Bit
                ? group_has_table64(@group.GroupHandle, tableName, (IntPtr) tableName.Length)
                : group_has_table32(@group.GroupHandle, tableName, (IntPtr) tableName.Length));
        }


        [DllImport(L64, EntryPoint = "tableview_get_subtable", CallingConvention = CallingConvention.Cdecl)
        ]
        private static extern IntPtr tableView_get_subtable64(TableViewHandle tableViewHandle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "tableview_get_subtable", CallingConvention = CallingConvention.Cdecl)
        ]
        private static extern IntPtr tableView_get_subtable32(TableViewHandle tableViewHandle, IntPtr columnIndex, IntPtr rowIndex);

        //called by TableViewHandle
        public static IntPtr TableViewGetSubTable(TableViewHandle tableViewHandle, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? tableView_get_subtable64(tableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : tableView_get_subtable32(tableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }

       
        //tightdb_c_cs_API void unbind_table_ref(const size_t TablePtr)

        [DllImport(L64, EntryPoint = "unbind_table_ref", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref64(TableHandle tableHandle);

        [DllImport(L32, EntryPoint = "unbind_table_ref", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind_table_ref32(TableHandle tableHandle);

        //      void    table_unbind(const Table *t); /* Ref-count delete of table* from table_get_table() */
        //called from TableHandle
        internal static void TableUnbind(TableHandle tableHandle)
        {
            if (Is64Bit)
                unbind_table_ref64(tableHandle);
            else
                unbind_table_ref32(tableHandle);
        }


        //TIGHTDB_C_CS_API void tableview_delete(TableView * tableview_ptr )

        [DllImport(L64, EntryPoint = "tableview_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_delete64(TableViewHandle tableViewHandle);

        [DllImport(L32, EntryPoint = "tableview_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_delete32(TableViewHandle tableViewHandle);

        //      void    table_unbind(const Table *t); /* Ref-count delete of table* from table_get_table() */
        //This should only be called from TableViewHandle
        public static void TableViewUnbind(TableViewHandle tvHandle)
        {
            if (Is64Bit)
                tableview_delete64(tvHandle);
            else
                tableview_delete32(tvHandle);
        }



        [DllImport(L64, EntryPoint = "tableview_clear", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_clear64(TableViewHandle tableViewHandle);

        [DllImport(L32, EntryPoint = "tableview_clear", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_clear32(TableViewHandle tableViewHandle);


        public static void TableViewClear(TableView tv)
        {
            if (Is64Bit)
                tableview_clear64(tv.TableViewHandle);
            else
                tableview_clear32(tv.TableViewHandle);
        }



        [DllImport(L64, EntryPoint = "tableview_remove_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_remove_row64(TableViewHandle tableViewHandle, long rowIndex);

        [DllImport(L32, EntryPoint = "tableview_remove_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_remove_row32(TableViewHandle tableViewHandle, long rowIndex);



        public static void TableViewRemoveRow(TableView tv, long rowIndex)
        {
            if (Is64Bit)
                tableview_remove_row64(tv.TableViewHandle, rowIndex);
            else
                tableview_remove_row32(tv.TableViewHandle, rowIndex);
        }


        [DllImport(L64, EntryPoint = "table_clear", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_clear64(TableHandle tableHandle);

        [DllImport(L32, EntryPoint = "table_clear", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_clear32(TableHandle tableHandle);


        public static void TableClear(Table table)
        {
            if (Is64Bit)
                table_clear64(table.TableHandle);
            else
                table_clear32(table.TableHandle);
        }




        [DllImport(L64, EntryPoint = "table_remove_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_remove_row64(TableHandle tableHandle, long rowIndex);

        [DllImport(L32, EntryPoint = "table_remove_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_remove_row32(TableHandle tableHandle, long rowIndex);

        public static void TableRemove(Table t, long rowIndex)
        {

            if (Is64Bit)
                table_remove_row64(t.TableHandle, rowIndex);
            else
                table_remove_row32(t.TableHandle, rowIndex);

        }



        [DllImport(L64, EntryPoint = "query_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_delete64(QueryHandle handle);

        [DllImport(L32, EntryPoint = "query_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_delete32(QueryHandle handle);

        //called by queryHandle to actually unbind a query, indirectly called by query's root
        public static void QueryDelete(QueryHandle queryHandle)
        {
            if (Is64Bit)
                query_delete64(queryHandle);
            else
                query_delete32(queryHandle);
        }


        [DllImport(L64, EntryPoint = "group_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void group_delete64(GroupHandle handle);

        [DllImport(L32, EntryPoint = "group_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void group_delete32(GroupHandle handle);

        //called by grouphandle 
        public static void GroupDelete(GroupHandle handle)
        {
            if (Is64Bit)
                group_delete64(handle);
            else
                group_delete32(handle);
        }


        [DllImport(L64, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_where64(TableHandle handle);

        [DllImport(L32, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_where32(TableHandle handle);

        //called by TableHandle
        public static IntPtr TableWhere(TableHandle tableHandle)
        {
            return Is64Bit
                ? table_where64(tableHandle)
                : table_where32(tableHandle);
        }



        // tightdb_c_cs_API size_t table_get_spec(size_t TablePtr)
        [DllImport(L64, EntryPoint = "table_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec64(TableHandle tableHandle);

        [DllImport(L32, EntryPoint = "table_get_spec", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_spec32(TableHandle tableHandle);

        //called by TableHandle in order to get an antomically set SpecHandle with IgnoreUnbind=true
        //todo:consider if we should create a base kind of spec handle, and two derived kinds of spec handle, one that unbinds and one that does not
        //then we could simply use them as spechandle and instantiate them as ignoreunbind or not
        public static IntPtr TableGetSpec(TableHandle tableHandle)
        {
            return (Is64Bit)
                ? table_get_spec64(tableHandle)
                : table_get_spec32(tableHandle);
        }



        //tightdb_c_cs_API size_t get_column_count(tightdb::Table* TablePtr)


        //            size_t      table_get_column_count(const Table *t);        
        [DllImport(L64, EntryPoint = "table_get_column_count", CallingConvention = CallingConvention.Cdecl)
        ]
        private static extern IntPtr table_get_column_count64(TableHandle tableHandle);

        [DllImport(L32, EntryPoint = "table_get_column_count", CallingConvention = CallingConvention.Cdecl)
        ]
        private static extern IntPtr table_get_column_count32(TableHandle tableHandle);

        public static long TableGetColumnCount(Table t)
        {
            return Is64Bit
                ? (long) table_get_column_count64(t.TableHandle)
                : (long) table_get_column_count32(t.TableHandle);
        }


        [DllImport(L64, EntryPoint = "tableview_get_column_count",CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_count64(TableViewHandle tableViewHandle);

        [DllImport(L32, EntryPoint = "tableview_get_column_count",CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_count32(TableViewHandle tableViewHandle);

        public static long TableViewGetColumnCount(TableView t)
        {
            return Is64Bit
                ? (long) tableView_get_column_count64(t.TableViewHandle)
                : (long) tableView_get_column_count32(t.TableViewHandle);
        }


        //            const char *table_get_column_name(const Table *t, size_t ndx);
        [DllImport(L64, EntryPoint = "table_get_column_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_name64(TableHandle tableHandle, IntPtr columnIndex, IntPtr buffer,
            IntPtr bufsize);

        [DllImport(L32, EntryPoint = "table_get_column_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_name32(TableHandle tableHandle, IntPtr columnIndex, IntPtr buffer,
            IntPtr bufsize);


        public static string TableGetColumnName(Table t, long columnIndex)

        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;

            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            table_get_column_name64(t.TableHandle, (IntPtr)columnIndex, buffer,
                                (IntPtr) currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (long)
                            table_get_column_name32(t.TableHandle, (IntPtr)columnIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
            //in c# this does NOT result in a copy, we get a string that points to the B buffer (If the now immutable string inside b is reused , it will get itself a new buffer
        }










        [DllImport(L64, EntryPoint = "tableview_get_column_name",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_name64(TableViewHandle handle, IntPtr columnIndex,
            IntPtr buffer,
            IntPtr bufsize);

        [DllImport(L32, EntryPoint = "tableview_get_column_name",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_name32(TableViewHandle handle, IntPtr columnIndex,
            IntPtr buffer,
            IntPtr bufsize);


        public static string TableViewGetColumnName(TableView tv, long columnIndex)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;

            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            tableView_get_column_name64(tv.TableViewHandle, (IntPtr)columnIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

                else
                    bufferSizeNeededChars =
                        (long)
                            tableView_get_column_name32(tv.TableViewHandle, (IntPtr)columnIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }



        [DllImport(L64, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_string64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_string32(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);


        public static string TableGetString(Table t, long columnIndex, long rowIndex)
            //ColumnIndex not a long bc on the c++ side it might be 32bit long on a 32 bit platform
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;

            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            table_get_string64(t.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

                else
                    bufferSizeNeededChars =
                        (long)
                            table_get_string32(t.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }





        //we get a memalloced' buffer back with the group data.
        //this buffer is copied to managed memory and a pointer to it is returned
        //before we return we call the dll to get the memalloce'd data freed
        //having the dll both allocate and deallocate the memory ensures that we don't do anything wrong
        //and ensures that if the memory allocation in core changes, only the dll must be changed
        [DllImport(L64, EntryPoint = "group_write_to_mem", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_write_to_mem64(GroupHandle groupPtr, out IntPtr size);

        [DllImport(L32, EntryPoint = "group_write_to_mem", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr group_write_to_mem32(GroupHandle groupPtr, out IntPtr size);


        public static byte[] GroupWriteToMemory(Group group)
        {
            IntPtr datalength;

            var data =
                (Is64Bit)
                    ? group_write_to_mem64(group.GroupHandle, out datalength)
                    : group_write_to_mem32(group.GroupHandle, out datalength);

            //now, datalength should contain number of bytes in data,
            //and data should be a pointer to those bytes
            //as data is managed on the c++ side, we will now copy data over to managed memory
            //if datalength is 0 marshal.copy wil copy nothing and we will return a byte[0]
            //after we have copied the data, we call group_write_to_mem_free to deallocate the data via the dll
            try
            {
                var numBytes = datalength.ToInt64();
                var ret = new byte[numBytes];
                Marshal.Copy(data, ret, 0, (int) datalength);
                return ret;
            }
            finally
            {
                if (data != IntPtr.Zero)
                {
                    if (Is64Bit)
                        group_write_to_mem_free64(data);
                    else
                        group_write_to_mem_free32(data);
                }
            }
        }

        [DllImport(L64, EntryPoint = "group_write_to_mem_free", CallingConvention = CallingConvention.Cdecl)]
        private static extern void group_write_to_mem_free64(IntPtr dataToFree);

        [DllImport(L32, EntryPoint = "group_write_to_mem_free", CallingConvention = CallingConvention.Cdecl)]
        private static extern void group_write_to_mem_free32(IntPtr dataToFree);






        //not using automatic marshalling (which might lead to copying in some cases),
        //The SizePtr variable must be a pointer to C# allocated and pinned memory where c++ can write the size
        //of the data (length in bytes)
        //the call will return a pointer to the array data, and the IntPtr that SizePtr pointed to will have changed
        //to contain the length of the data
        //the data that is owned in c++ is okay to read as long as we don't change the table, and as we do not change tables concurrently we are
        //safe to access the returned byte array to get us a copy of the data towards the end of this method

        [DllImport(L64, EntryPoint = "table_get_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_binary64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            out IntPtr size);

        [DllImport(L32, EntryPoint = "table_get_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_binary32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            out IntPtr size);


        public static byte[] TableGetBinary(Table table, long columnIndex, long rowIndex)
        {
            IntPtr datalength;

            IntPtr data =
                (Is64Bit)
                    ? table_get_binary64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, out datalength)
                    : table_get_binary32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, out datalength);

            //now, datalength should contain number of bytes in data,
            //and data should be a pointer to those bytes
            //as data is managed on the c++ side, we will now copy data over to managed memory
            //if datalength is 0 marshal.copy wil copy nothing and we will return a byte[0]
            var numBytes = datalength.ToInt64();
            var ret = new byte[numBytes];
            Marshal.Copy(data, ret, 0, (int) datalength);
            return ret;
        }


        //not using automatic marshalling (which might lead to copying in some cases),
        //The SizePtr variable must be a pointer to C# allocated and pinned memory where c++ can write the size
        //of the data (length in bytes)
        //the call will return a pointer to the array data, and the IntPtr that SizePtr pointed to will have changed
        //to contain the length of the data
        [DllImport(L64, EntryPoint = "table_get_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_binary64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            out IntPtr size);

        [DllImport(L32, EntryPoint = "table_get_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_binary32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            out IntPtr size);


        public static byte[] TableGetMixedBinary(Table table, long columnIndex, long rowIndex)
        {
            IntPtr datalength;

            var data =
                (Is64Bit)
                    ? table_get_mixed_binary64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        out datalength)
                    : table_get_mixed_binary32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        out datalength);

            //now, datalength should contain number of bytes in data,
            //and data should be a pointer to those bytes
            //as data is managed on the c++ side, we will now copy data over to managed memory
            //if datalength is 0 marshal.copy wil copy nothing and we will return a byte[0]
            var numBytes = datalength.ToInt64();
            var ret = new byte[numBytes];
            Marshal.Copy(data, ret, 0, (int) datalength);
            return ret;
        }






        [DllImport(L64, EntryPoint = "table_get_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_string64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "table_get_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_string32(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);


        public static string TableGetMixedString(Table t, long columnIndex, long rowIndex)
            //ColumnIndex not a long bc on the c++ side it might be 32bit long on a 32 bit platform
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;


            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            table_get_mixed_string64(t.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

                else
                    bufferSizeNeededChars =
                        (long)
                            table_get_mixed_string32(t.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, buffer,
                                (IntPtr) currentBufferSizeChars);


            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }




        [DllImport(L64, EntryPoint = "tableview_get_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_mixed_string64(TableViewHandle handle, IntPtr columnIndex,
            IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_mixed_string32(TableViewHandle handle, IntPtr columnIndex,
            IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);


        public static string TableViewGetMixedString(TableView t, long columnIndex, long rowIndex)
            //ColumnIndex not a long bc on the c++ side it might be 32bit long on a 32 bit platform
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;


            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            tableview_get_mixed_string64(t.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                                buffer,
                                (IntPtr) currentBufferSizeChars);

                else
                    bufferSizeNeededChars =
                        (long)
                            tableview_get_mixed_string32(t.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                                buffer,
                                (IntPtr) currentBufferSizeChars);


            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }




        //not using automatic marshalling (which might lead to copying in some cases),
        //The SizePtr variable must be a pointer to C# allocated and pinned memory where c++ can write the size
        //of the data (length in bytes)
        //the call will return a pointer to the array data, and the IntPtr that SizePtr pointed to will have changed
        //to contain the length of the data
        [DllImport(L64, EntryPoint = "tableview_get_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_mixed_binary64(TableViewHandle tablePtr, IntPtr columnNdx,
            IntPtr rowNdx,
            out IntPtr size);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_mixed_binary32(TableViewHandle tablePtr, IntPtr columnNdx,
            IntPtr rowNdx,
            out IntPtr size);


        public static byte[] TableViewGetMixedBinary(TableView tableView, long columnIndex, long rowIndex)
        {
            IntPtr datalength;

            var data =
                (Is64Bit)
                    ? tableview_get_mixed_binary64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        out datalength)
                    : tableview_get_mixed_binary32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        out datalength);

            //now, datalength should contain number of bytes in data,
            //and data should be a pointer to those bytes
            //as data is managed on the c++ side, we will now copy data over to managed memory
            //if datalength is 0 marshal.copy wil copy nothing and we will return a byte[0]
            var numBytes = datalength.ToInt64();
            var ret = new byte[numBytes];
            Marshal.Copy(data, ret, 0, (int) datalength);
            return ret;
        }





        //this is a commented version of a typical string returning call. Other calls are without comments
        //documented string returning call. string return
        [DllImport(L64, EntryPoint = "tableview_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_string64(TableViewHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer,
            IntPtr bufsize);

        [DllImport(L32, EntryPoint = "tableview_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_string32(TableViewHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer,
            IntPtr bufsize);


        public static string TableviewGetString(TableView tv, long columnIndex, long rowIndex)

        {
            long bufferSizeNeededChars = 16;
            //in 16 bit chars - in reality won't go near 2^32 as .net only allows 2GB structures anyways and this is 16Bit elements so about 2^31 elements max           
            IntPtr buffer; //unmanaged buffer for c++ to put an utf-16 string into (on .net)
            long currentBufferSizeChars; //holds the size of the buffer right now
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                //allocates buffer on the heap, and sets currentbuffersizechars to the size.             
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            tableview_get_string64(tv.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

                else
                    bufferSizeNeededChars =
                        (long)
                            tableview_get_string32(tv.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, buffer,
                                (IntPtr) currentBufferSizeChars);


            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            //return true if buffer was too small, also deallocates the too small buffer

            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
            //converts buffer to a C# string and return it, also deallocates the unmanaged buffer
        }




        //not using automatic marshalling (which might lead to copying in some cases),
        //The SizePtr variable must be a pointer to C# allocated and pinned memory where c++ can write the size
        //of the data (length in bytes)
        //the call will return a pointer to the array data, and the IntPtr that SizePtr pointed to will have changed
        //to contain the length of the data
        [DllImport(L64, EntryPoint = "tableview_get_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_binary64(TableViewHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            out IntPtr size);

        [DllImport(L32, EntryPoint = "tableview_get_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_binary32(TableViewHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            out IntPtr size);


        public static byte[] TableViewGetBinary(TableView tableView, long columnIndex, long rowIndex)
        {
            IntPtr datalength;

            var data =
                (Is64Bit)
                    ? tableview_get_binary64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        out datalength)
                    : tableview_get_binary32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        out datalength);

            //now, datalength should contain number of bytes in data,
            //and data should be a pointer to those bytes
            //as data is managed on the c++ side, we will now copy data over to managed memory
            //if datalength is 0 marshal.copy wil copy nothing and we will return a byte[0]
            var numBytes = datalength.ToInt64();
            var ret = new byte[numBytes];
            Marshal.Copy(data, ret, 0, (int) datalength);
            return ret;
        }













        [DllImport(L64, EntryPoint = "spec_get_column_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_name64(SpecHandle specHandle, IntPtr columnIndex,
            IntPtr b, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "spec_get_column_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr spec_get_column_name32(SpecHandle specHandle, IntPtr columnIndex,
            IntPtr b, IntPtr bufsize);


        public static String SpecGetColumnName(Spec spec, long columnIndex)
        {

            long bufferSizeNeededChars = 16;
            //in 16 bit chars - in reality won't go near 2^32 as .net only allows 2GB structures anyways and this is 16Bit elements so about 2^31 elements max           
            IntPtr buffer; //unmanaged buffer for c++ to put an utf-16 string into (on .net)
            long currentBufferSizeChars; //holds the size of the buffer right now
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            spec_get_column_name64(spec.SpecHandle, (IntPtr) columnIndex, buffer,
                                (IntPtr) currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (long)
                            spec_get_column_name32(spec.SpecHandle, (IntPtr) columnIndex, buffer,
                                (IntPtr) currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));

            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }





        [DllImport(L64, EntryPoint = "table_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_type64(TableHandle tablePtr, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "table_get_column_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_column_type32(TableHandle tablePtr, IntPtr columnIndex);

        public static DataType TableGetColumnType(Table t, long columnIndex)
        {
            return IntPtrToDataType(
                Is64Bit
                    ? table_get_column_type64(t.TableHandle, (IntPtr) columnIndex)
                    : table_get_column_type32(t.TableHandle, (IntPtr) columnIndex));
        }

        [DllImport(L64, EntryPoint = "tableview_get_column_type",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_type64(TableViewHandle tableViewPtr, IntPtr columnIndex);

        [DllImport(L32, EntryPoint = "tableview_get_column_type",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_column_type32(TableViewHandle tableViewPtr, IntPtr columnIndex);

        public static DataType TableViewGetColumnType(TableView tv, long columnIndex)
        {
            return IntPtrToDataType(
                Is64Bit
                    ? tableView_get_column_type64(tv.TableViewHandle, (IntPtr) columnIndex)
                    : tableView_get_column_type32(tv.TableViewHandle, (IntPtr) columnIndex));
        }


        //we have to trust that c++ DataType fills up the same amount of stack space as one of our own DataType enum's
        //This is the case on windows, visual studio2010 and 2012 but Who knows if some c++ compiler somewhere someday decides to store DataType differently       
        //
        [DllImport(L64, EntryPoint = "tableview_get_mixed_type",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_mixed_type64(TableViewHandle tablePtr, IntPtr columnIndex,
            IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_type",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_mixed_type32(TableViewHandle tablePtr, IntPtr columnIndex,
            IntPtr rowIndex);

        public static DataType TableViewGetMixedType(TableView t, long columnIndex, long rowIndex)
        {
            return IntPtrToDataType(Is64Bit
                ? tableView_get_mixed_type64(t.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : tableView_get_mixed_type32(t.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex));
        }


        //we have to trust that c++ DataType fills up the same amount of stack space as one of our own DataType enum's
        //This is the case on windows, visual studio2010 and 2012 but Who knows if some c++ compiler somewhere someday decides to store DataType differently

        //
        [DllImport(L64, EntryPoint = "table_get_mixed_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_type64(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "table_get_mixed_type", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_type32(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex);

        public static DataType TableGetMixedType(Table t, long columnIndex, long rowIndex)
        {
            return IntPtrToDataType(Is64Bit
                ? table_get_mixed_type64(t.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : table_get_mixed_type32(t.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex));
        }


        [DllImport(L64, EntryPoint = "table_has_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_has_index64(TableHandle tablePtr, IntPtr columnNdx);

        [DllImport(L32, EntryPoint = "table_has_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_has_index32(TableHandle tablePtr, IntPtr columnNdx);

        public static bool TableHasIndex(Table table, long columnIndex)
        {
            return
                IntPtrToBool(Is64Bit
                    ? table_has_index64(table.TableHandle, (IntPtr) columnIndex)
                    : table_has_index32(table.TableHandle, (IntPtr) columnIndex));
        }




        [DllImport(L64, EntryPoint = "table_insert_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_empty_row64(TableHandle tablePtr, IntPtr rowIndex, IntPtr numRows);

        [DllImport(L32, EntryPoint = "table_insert_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_empty_row32(TableHandle tablePtr, IntPtr rowIndex, IntPtr numRows);

        public static void TableInsertEmptyRow(Table table, long rowIndex, long numberOfRows)
        {
            if (Is64Bit)
                table_insert_empty_row64(table.TableHandle, (IntPtr) rowIndex, (IntPtr) numberOfRows);
            else
                table_insert_empty_row32(table.TableHandle, (IntPtr) rowIndex, (IntPtr) numberOfRows);
        }



        //TIGHTDB_C_CS_API size_t table_add_empty_row(Table* TablePtr, size_t num_rows)

        [DllImport(L64, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row64(TableHandle tablePtr, IntPtr numRows);

        [DllImport(L32, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row32(TableHandle tablePtr, IntPtr numRows);

        public static long TableAddEmptyRow(Table table, long numberOfRows)
        {
            return Is64Bit
                ? (long) table_add_empty_row64(table.TableHandle, (IntPtr) numberOfRows)
                : (long) table_add_empty_row32(table.TableHandle, (IntPtr) numberOfRows);
        }


        //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport(L64, EntryPoint = "table_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);

        [DllImport(L32, EntryPoint = "table_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_int32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);

        public static void TableSetLong(Table table, long columnIndex, long rowIndex, long value)
        {

            if (Is64Bit)
                table_set_int64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                table_set_int32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }


        //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport(L64, EntryPoint = "table_set_32int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_32int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, int value);

        [DllImport(L32, EntryPoint = "table_set_32int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_32int32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, int value);

        public static void TableSetInt(Table table, long columnIndex, long rowIndex, int value)
        {
            if (Is64Bit)
                table_set_32int64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                table_set_32int32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }





        //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport(L64, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_string64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_string32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        public static void TableSetString(Table table, long columnIndex, long rowIndex, string value)
        {

            if (Is64Bit)
                table_set_string64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
            else
                table_set_string32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
        }


        [DllImport(L64, EntryPoint = "table_set_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_string64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "table_set_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_string32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        public static void TableSetMixedString(Table table, long columnIndex, long rowIndex, string value)
        {

            if (Is64Bit)
                table_set_mixed_string64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
            else
                table_set_mixed_string32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
        }



        [DllImport(L64, EntryPoint = "tableview_set_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_string64(TableViewHandle tablePtr, IntPtr columnNdx,
            IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_string32(TableViewHandle tablePtr, IntPtr columnNdx,
            IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        public static void TableViewSetMixedString(TableView tableView, long columnIndex, long rowIndex, string value)
        {

            if (Is64Bit)
                tableview_set_mixed_string64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
            else
                tableview_set_mixed_string32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
        }







        //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport(L64, EntryPoint = "tableview_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_string64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(L32, EntryPoint = "tableview_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_string32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        public static void TableViewSetString(TableView tableView, long columnIndex, long rowIndex, string value)
        {

            if (Is64Bit)
                tableView_set_string64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
            else
                tableView_set_string32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value,
                    (IntPtr) value.Length);
        }






        //not using automatic marshalling (which might lead to copying in some cases),
        //but ensuring no copying of the array data is done, by getting a pinned pointer to the array supplied by the user.
        [DllImport(L64, EntryPoint = "tableview_set_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_binary64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value, IntPtr bytes);

        [DllImport(L32, EntryPoint = "tableview_set_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_binary32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value, IntPtr bytes);


        public static void TableViewSetBinary(TableView tableView, long columnIndex, long rowIndex, Byte[] value)
        {
            //special case if we get called with null (which means add a binarydata of size 0
            //core will ignore the pointer and concentrate on size=0
            //reason for the special case is an easy way around the try finally block 
            if (value == null)
            {
                if (Is64Bit)
                    tableview_set_binary64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        IntPtr.Zero,
                        IntPtr.Zero);
                else
                    tableview_set_binary32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        IntPtr.Zero,
                        IntPtr.Zero);
                return;
            }

            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            //now value cannot be moved or garbage collected by garbage collector
            var valuePointer = handle.AddrOfPinnedObject();
            try
            {
                if (Is64Bit)
                    tableview_set_binary64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        valuePointer,
                        (IntPtr) value.Length);
                else
                    tableview_set_binary32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        valuePointer,
                        (IntPtr) value.Length);
            }
            finally
            {
                handle.Free(); //allow Garbage collector to move and deallocate value as it wishes
            }
        }



        [DllImport(L64, EntryPoint = "tableview_set_32int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_32int64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            int value);

        [DllImport(L32, EntryPoint = "tableview_set_32int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_32int32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            int value);

        public static void TableViewSetInt(TableView tableView, long columnIndex, long rowIndex, int value)
        {

            if (Is64Bit)
                tableView_set_32int64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                tableView_set_32int32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }







        [DllImport(L64, EntryPoint = "tableview_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_int64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            long value);

        [DllImport(L32, EntryPoint = "tableview_set_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_int32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            long value);

        public static void TableViewSetLong(TableView tableView, long columnIndex, long rowIndex, long value)
        {

            if (Is64Bit)
                tableView_set_int64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                tableView_set_int32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }


        [DllImport(L64, EntryPoint = "table_add_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_add_int64(TableHandle tablePtr, IntPtr columnNdx, long value);

        [DllImport(L32, EntryPoint = "table_add_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_add_int32(TableHandle tablePtr, IntPtr columnNdx, long value);

        public static void TableAddInt(Table table, long columnIndex, long value)
        {
            if (Is64Bit)
                table_add_int64(table.TableHandle, (IntPtr) columnIndex, value);
            else
                table_add_int32(table.TableHandle, (IntPtr) columnIndex, value);
        }

        //todo implement tableView.AddInt when it has been supported in core


        [DllImport(L64, EntryPoint = "table_set_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_subtable64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            TableHandle sourceTablePtr);

        [DllImport(L32, EntryPoint = "table_set_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_subtable32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            TableHandle sourceTablePtr);

        //todo:could core throw an error?  introduce a way of getting such an error over here, and handle it
        public static void TableSetSubTable(Table table, long columnIndex, long rowIndex, Table sourceTable)
        {
            if (Is64Bit)
                table_set_subtable64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, sourceTable.TableHandle);
            else
                table_set_subtable32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, sourceTable.TableHandle);
        }



        [DllImport(L64, EntryPoint = "tableview_set_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_subtable64(TableViewHandle tableviewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            TableHandle sourceTablePtr);

        [DllImport(L32, EntryPoint = "tableview_set_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_subtable32(TableViewHandle tableviewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            TableHandle sourceTablePtr);

        public static void TableViewSetSubTable(TableView tableView, long columnIndex, long rowIndex, Table sourceTable)
        {
            if (Is64Bit)
                tableview_set_subtable64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                    sourceTable.TableHandle);
            else
                tableview_set_subtable32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                    sourceTable.TableHandle);
        }


        //TIGHTDB_C_CS_API void table_set_mixed_subtable(tightdb::Table* table_ptr,size_t col_ndx, size_t row_ndx,Table* source);
        [DllImport(L64, EntryPoint = "table_set_mixed_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_subtable64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            TableHandle sourceTablePtr);

        [DllImport(L32, EntryPoint = "table_set_mixed_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_subtable32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            TableHandle sourceTablePtr);

        public static void TableSetMixedSubTable(Table table, long columnIndex, long rowIndex, Table sourceTable)
        {
            if (Is64Bit)
                table_set_mixed_subtable64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                    sourceTable.TableHandle);
            else
                table_set_mixed_subtable32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                    sourceTable.TableHandle);
        }


        [DllImport(L64, EntryPoint = "tableview_set_mixed_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_mixed_subtable64(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            TableHandle sourceTablePtr);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_mixed_subtable32(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            TableHandle sourceTablePtr);

        public static void TableViewSetMixedSubTable(TableView tableView, long columnIndex, long rowIndex,
            Table sourceTable)
        {
            if (Is64Bit)
                tableView_set_mixed_subtable64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                    sourceTable.TableHandle);
            else
                tableView_set_mixed_subtable32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                    sourceTable.TableHandle);
        }



        //not using automatic marshalling (which might lead to copying in some cases),
        //but ensuring no copying of the array data is done, by getting a pinned pointer to the array supplied by the user.
        [DllImport(L64, EntryPoint = "table_set_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_binary64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value,
            IntPtr bytes);

        [DllImport(L32, EntryPoint = "table_set_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_binary32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value,
            IntPtr bytes);


        public static void TableSetBinary(Table table, long columnIndex, long rowIndex, Byte[] value)
        {
            //special case if we get called with null (which means add a binarydata of size 0
            //core will ignore the pointer and concentrate on size=0
            if (value == null)
            {
                if (Is64Bit)
                    table_set_binary64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, IntPtr.Zero,
                        IntPtr.Zero);
                else
                    table_set_binary32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, IntPtr.Zero,
                        IntPtr.Zero);
                return;
            }

            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            //now value cannot be moved or garbage collected by garbage collector
            var valuePointer = handle.AddrOfPinnedObject();
            try
            {
                if (Is64Bit)
                    table_set_binary64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, valuePointer,
                        (IntPtr) value.Length);
                else
                    table_set_binary32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, valuePointer,
                        (IntPtr) value.Length);
            }
            finally
            {
                handle.Free(); //allow Garbage collector to move and deallocate value as it wishes
            }
        }





        //not using automatic marshalling (which might lead to copying in some cases),
        //but ensuring no copying of the array data is done, by getting a pinned pointer to the array supplied by the user.
        //Note that the construction is happening in the interop call and not inside a tightdbhandle as usual.
        //This is because root should be set to null always anyway, and that is the default value, so just having the
        //handle set atomically with guarentee by CriticalHandle is good enough
        [DllImport(L64, EntryPoint = "group_from_binary_data", CallingConvention = CallingConvention.Cdecl)]
        private static extern GroupHandle group_from_binary_data64(IntPtr value, IntPtr bytes);

        [DllImport(L32, EntryPoint = "group_from_binary_data", CallingConvention = CallingConvention.Cdecl)]
        private static extern GroupHandle group_from_binary_data32(IntPtr value, IntPtr bytes);


        //calling this method with NULL is illegal and will crash the system, so stuff calling this method must validate for null first
        public static void GroupFrombinaryData(Group group, Byte[] data)
        {
            Debug.Assert(data != null);
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            //now value cannot be moved or garbage collected by garbage collector
            var dataPointer = handle.AddrOfPinnedObject();
            try
            {
                var groupHandle = (Is64Bit)
                    ? group_from_binary_data64(dataPointer, (IntPtr) data.Length)
                    : group_from_binary_data32(dataPointer, (IntPtr) data.Length);
                    //in this call, groupHandle is guarenteed to be set and is able to finalize itself

                if (groupHandle.IsInvalid)
                    throw new ArgumentException("GroupFromBinaryData called with invalid binary data");

                group.SetHandle(groupHandle, false); //from binary returns a RW group
            }
            finally
            {
                handle.Free(); //allow Garbage collector to move and deallocate value as it wishes
            }
        }














        //not using automatic marshalling (which might lead to copying in some cases),
        //but ensuring no copying of the array data is done, by getting a pinned pointer to the array supplied by the user.
        [DllImport(L64, EntryPoint = "table_set_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_binary64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value, IntPtr bytes);

        [DllImport(L32, EntryPoint = "table_set_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_binary32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value, IntPtr bytes);


        public static void TableSetMixedBinary(Table table, long columnIndex, long rowIndex, Byte[] value)
        {

            if (value == null)
            {
                if (Is64Bit)
                    table_set_mixed_binary64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, IntPtr.Zero,
                        IntPtr.Zero);
                else
                    table_set_mixed_binary32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, IntPtr.Zero,
                        IntPtr.Zero);
                return;
            }


            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            //now value cannot be moved or garbage collected by garbage collector
            var valuePointer = handle.AddrOfPinnedObject();
            try
            {
                if (Is64Bit)
                    table_set_mixed_binary64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, valuePointer,
                        (IntPtr) value.Length);
                else
                    table_set_mixed_binary32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, valuePointer,
                        (IntPtr) value.Length);
            }
            finally
            {
                handle.Free(); //allow Garbage collector to move and deallocate value as it wishes
            }
        }



        //not using automatic marshalling (which might lead to copying in some cases),
        //but ensuring no copying of the array data is done, by getting a pinned pointer to the array supplied by the user.
        [DllImport(L64, EntryPoint = "tableview_set_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_binary64(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            IntPtr value, IntPtr bytes);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_binary32(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            IntPtr value, IntPtr bytes);

        public static void TableViewSetMixedBinary(TableView tableView, long columnIndex, long rowIndex, Byte[] value)
        {


            if (value == null)
            {
                if (Is64Bit)
                    tableview_set_mixed_binary64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        IntPtr.Zero,
                        IntPtr.Zero);
                else
                    tableview_set_mixed_binary32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        IntPtr.Zero,
                        IntPtr.Zero);
                return;
            }

            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            //now value cannot be moved or garbage collected by garbage collector
            var valuePointer = handle.AddrOfPinnedObject();
            try
            {
                if (Is64Bit)
                    tableview_set_mixed_binary64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        valuePointer,
                        (IntPtr) value.Length);
                else
                    tableview_set_mixed_binary32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex,
                        valuePointer,
                        (IntPtr) value.Length);
            }
            finally
            {
                handle.Free(); //allow Garbage collector to move and deallocate value as it wishes
            }
        }


        [DllImport(L64, EntryPoint = "table_set_mixed_empty_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_empty_subtable64(TableHandle tablePtr, IntPtr columnNdx,
            IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_set_mixed_empty_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_empty_subtable32(TableHandle tablePtr, IntPtr columnNdx,
            IntPtr rowNdx);

        public static void TableSetMixedEmptySubTable(Table table, long columnIndex, long rowIndex)
        {

            if (Is64Bit)
                table_set_mixed_empty_subtable64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
            else
                table_set_mixed_empty_subtable32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }



        [DllImport(L64, EntryPoint = "tableview_set_mixed_empty_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_mixed_empty_subtable64(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_empty_subtable",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_mixed_empty_subtable32(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx);

        public static void TableViewSetMixedEmptySubTable(TableView tableView, long columnIndex, long rowIndex)
        {
            if (Is64Bit)
                tableView_set_mixed_empty_subtable64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
            else
                tableView_set_mixed_empty_subtable32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }







        [DllImport(L64, EntryPoint = "tableview_set_mixed_int", CallingConvention = CallingConvention.Cdecl
            )]
        private static extern void tableView_set_mixed_int64(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            long value);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_int", CallingConvention = CallingConvention.Cdecl
            )]
        private static extern void tableView_set_mixed_int32(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx,
            long value);

        public static void TableViewSetMixedLong(TableView tableView, long columnIndex, long rowIndex, long value)
        {

            if (Is64Bit)
                tableView_set_mixed_int64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                tableView_set_mixed_int32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }

        //just calls the version with the long parameter. We  might want to add a function that takes
        //an int instead of a long, but current speed tests show neglible difference
        public static void TableViewSetMixedInt(TableView tableView, long columnIndex, long rowIndex, int value)
        {
            if (Is64Bit)
                tableView_set_mixed_int64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                tableView_set_mixed_int32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }


        [DllImport(L64, EntryPoint = "table_set_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            long value);

        [DllImport(L32, EntryPoint = "table_set_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_int32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            long value);

        public static void TableSetMixedLong(Table table, long columnIndex, long rowIndex, long value)
        {
            if (Is64Bit)
                table_set_mixed_int64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                table_set_mixed_int32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }



        [DllImport(L64, EntryPoint = "table_set_mixed_int32", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_int3264(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            int value);

        [DllImport(L32, EntryPoint = "table_set_mixed_int32", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_int3232(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            int value);

        public static void TableSetMixedInt(Table table, long columnIndex, long rowIndex, int value)
        {
            if (Is64Bit)
                table_set_mixed_int3264(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                table_set_mixed_int3232(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }

        /*  no used for now
        //        TIGHTDB_C_CS_API void insert_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport(L64, EntryPoint = "table_insert_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_int64(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);

        [DllImport(L32, EntryPoint = "table_insert_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_insert_int32(IntPtr tablePtr, IntPtr columnNdx, IntPtr rowNdx, long value);

        public static void TableInsertInt(Table table, long columnIndex, long rowIndex, long value)
        {

            if (Is64Bit)
                table_insert_int64(table.Handle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
            else
                table_insert_int32(table.Handle, (IntPtr) columnIndex, (IntPtr) rowIndex, value);
        }

        */

        //TIGHTDB_C_CS_API int64_t table_get_int(Table* TablePtr, size_t column_ndx, size_t row_ndx)
        [DllImport(L64, EntryPoint = "table_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_int32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);


        //note even though it's called getint - it does return an int64_t which is a 64 bit signed, that is, similar to C# long
        public static long TableGetInt(Table table, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? table_get_int64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : table_get_int32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }


        //TIGHTDB_C_CS_API int64_t table_get_int(Table* TablePtr, size_t column_ndx, size_t row_ndx)
        [DllImport(L64, EntryPoint = "tableview_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableView_get_int64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long tableView_get_int32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);


        public static long TableViewGetInt(TableView tableView, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? tableView_get_int64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : tableView_get_int32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }



        //get an int from a mixed
        [DllImport(L64, EntryPoint = "table_get_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_mixed_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_mixed_int", CallingConvention = CallingConvention.Cdecl)]
        private static extern long table_get_mixed_int32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);


        //note even though it's called getint - it does return an int64_t which is a 64 bit signed, that is, similar to C# long
        public static long TableGetMixedInt(Table table, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? table_get_mixed_int64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : table_get_mixed_int32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }


        [DllImport(L64, EntryPoint = "table_get_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_bool64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_mixed_bool32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static bool TableGetMixedBool(Table table, long columnIndex, long rowIndex)
        {
            return IntPtrToBool(Is64Bit
                ? table_get_mixed_bool64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : table_get_mixed_bool32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex));
        }


        //get an int from a mixed
        [DllImport(L64, EntryPoint = "tableview_get_mixed_int", CallingConvention = CallingConvention.Cdecl
            )]
        private static extern long tableView_get_mixed_int64(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_int", CallingConvention = CallingConvention.Cdecl
            )]
        private static extern long tableView_get_mixed_int32(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx);


        //note even though it's called getint - it does return an int64_t which is a 64 bit signed, that is, similar to C# long
        public static long TableViewGetMixedInt(TableView tableView, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? tableView_get_mixed_int64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : tableView_get_mixed_int32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_get_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_mixed_bool64(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_mixed_bool32(TableViewHandle tableViewPtr, IntPtr columnNdx,
            IntPtr rowNdx);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static bool TableViewGetMixedBool(TableView tableView, long columnIndex, long rowIndex)
        {
            return IntPtrToBool(Is64Bit
                ? tableview_get_mixed_bool64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : tableview_get_mixed_bool32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex));
        }



        [DllImport(L64, EntryPoint = "table_get_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double table_get_double64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double table_get_double32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);


        //note even though it's called getint - it does return an int64_t which is a 64 bit signed, that is, similar to C# long
        public static Double TableGetDouble(Table table, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? table_get_double64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : table_get_double32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }


        //TIGHTDB_C_CS_API int64_t table_get_int(Table* TablePtr, size_t column_ndx, size_t row_ndx)
        [DllImport(L64, EntryPoint = "tableview_get_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double tableView_get_double64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double tableView_get_double32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);


        public static Double TableViewGetDouble(TableView tableView, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? tableView_get_double64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                : tableView_get_double32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }




        [DllImport(L64, EntryPoint = "table_get_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_get_float64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_get_float32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);
        

        public static float TableGetFloat(Table table, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? table_get_float64(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : table_get_float32(table.TableHandle, (IntPtr) columnIndex, (IntPtr) rowIndex);
        }


        
        [DllImport(L64, EntryPoint = "tableview_get_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableView_get_float64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableView_get_float32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);


        public static float TableViewGetFloat(TableView tableView, long columnIndex, long rowIndex)
        {
            return Is64Bit
                ? tableView_get_float64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                : tableView_get_float32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }







        //dates are always 64 bit in core
        [DllImport(L64, EntryPoint = "tableview_get_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 tableView_get_date64(TableViewHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 tableView_get_date32(TableViewHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        public static DateTime TableViewGetDateTime(TableView tableView, long columnIndex, long rowIndex)
        {
            var cppdate = Is64Bit
                ? tableView_get_date64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                : tableView_get_date32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            return ToCSharpTimeUtc(cppdate);
        }

        [DllImport(L64, EntryPoint = "table_get_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 table_get_date64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 table_get_date32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        public static DateTime TableGetDateTime(Table table, long columnIndex, long rowIndex)
        {

            var cppdate =
                Is64Bit
                    ? table_get_date64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                    : table_get_date32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            return ToCSharpTimeUtc(cppdate);
        }



        [DllImport(L64, EntryPoint = "tableview_get_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 tableView_get_mixed_date64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 tableView_get_mixed_date32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        public static DateTime TableViewGetMixedDateTime(TableView tableView, long columnIndex, long rowIndex)
        {
            var cppdate = Is64Bit
                ? tableView_get_mixed_date64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                : tableView_get_mixed_date32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            return ToCSharpTimeUtc(cppdate);
        }

        //the call should return a time_t in 64 bit format (always) so we marshal it as long
        [DllImport(L64, EntryPoint = "table_get_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 table_get_mixed_date64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 table_get_mixed_date32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        public static DateTime TableGetMixedDateTime(Table table, long columnIndex, long rowIndex)
        {
            var cppdate =
                Is64Bit
                    ? table_get_mixed_date64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                    : table_get_mixed_date32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
            return ToCSharpTimeUtc(cppdate);
        }




        [DllImport(L64, EntryPoint = "table_get_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double table_get_mixed_double64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double table_get_mixed_double32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        public static Double TableGetMixedDouble(Table table, long columnIndex, long rowIndex)
        {
            return
                Is64Bit
                    ? table_get_mixed_double64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                    : table_get_mixed_double32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }


        //the call should return a time_t in 64 bit format (always) so we marshal it as long
        [DllImport(L64, EntryPoint = "tableview_get_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double tableView_get_mixed_double64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double tableView_get_mixed_double32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        public static Double TableViewGetMixedDouble(TableView tableView, long columnIndex, long rowIndex)
        {
            return
                Is64Bit
                    ? tableView_get_mixed_double64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                    : tableView_get_mixed_double32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }




        [DllImport(L64, EntryPoint = "table_get_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_get_mixed_float64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float table_get_mixed_float32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        public static float TableGetMixedFloat(Table table, long columnIndex, long rowIndex)
        {
            return
                Is64Bit
                    ? table_get_mixed_float64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                    : table_get_mixed_float32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }




        //the call should return a time_t in 64 bit format (always) so we marshal it as long
        [DllImport(L64, EntryPoint = "tableview_get_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableView_get_mixed_float64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float tableView_get_mixed_float32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        public static float TableViewGetMixedFloat(TableView tableView, long columnIndex, long rowIndex)
        {
            return
                Is64Bit
                    ? tableView_get_mixed_float64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex)
                    : tableView_get_mixed_float32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex);
        }




        [DllImport(L64, EntryPoint = "table_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_size64(TableHandle tablePtr);

        [DllImport(L32, EntryPoint = "table_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_size32(TableHandle tablePtr);

        public static long TableSize(Table t)
        {
            return Is64Bit
                ? (long) table_get_size64(t.TableHandle)
                : (long) table_get_size32(t.TableHandle);
        }


        [DllImport(L64, EntryPoint = "test_size_calls", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_calls64();

        [DllImport(L32, EntryPoint = "test_size_calls", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_calls32();

        public static long TestSizeCalls()
        {
            return Is64Bit
                ? (long) test_size_calls64()
                : (long) test_size_calls32();
        }


        [DllImport(L64, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_bool64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_bool32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static bool TableGetBool(Table table, long columnIndex, long rowIndex)
        {
            if (Is64Bit)
                return IntPtrToBool(table_get_bool64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
            return IntPtrToBool(table_get_bool32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
        }



        [DllImport(L64, EntryPoint = "tableview_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_bool64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableView_get_bool32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static bool TableViewGetBool(TableView tableView, long columnIndex, long rowIndex)
        {
            return IntPtrToBool(Is64Bit
                ? tableView_get_bool64(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex)
                : tableView_get_bool32(tableView.TableViewHandle, (IntPtr) columnIndex, (IntPtr) rowIndex));
        }


        [DllImport(L64, EntryPoint = "tableview_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_bool64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        [DllImport(L32, EntryPoint = "tableview_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_bool32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static void TableViewSetBool(TableView tableView, long columnIndex, long rowIndex, Boolean value)
        {
            var ipValue = BoolToIntPtr(value);
            if (Is64Bit)
                tableView_set_bool64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
            else
                tableView_set_bool32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
        }

        [DllImport(L64, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_bool64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        [DllImport(L32, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_bool32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static void TableSetBool(Table table, long columnIndex, long rowIndex, Boolean value)
        {
            var ipValue = BoolToIntPtr(value);
            if (Is64Bit)
                table_set_bool64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
            else
                table_set_bool32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
        }



        [DllImport(L64, EntryPoint = "tableview_set_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_bool64(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_bool32(TableViewHandle tableViewPtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static void TableViewSetMixedBool(TableView tableView, long columnIndex, long rowIndex, Boolean value)
        {
            var ipValue = BoolToIntPtr(value);
            if (Is64Bit)
                tableview_set_mixed_bool64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
            else
                tableview_set_mixed_bool32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
        }

        [DllImport(L64, EntryPoint = "table_set_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_bool64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        [DllImport(L32, EntryPoint = "table_set_mixed_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_bool32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            IntPtr value);

        //convert.tobool does not take an IntPtr so we have to convert ourselves we get 1 for true, 0 for false
        public static void TableSetMixedBool(Table table, long columnIndex, long rowIndex, Boolean value)
        {
            var ipValue = BoolToIntPtr(value);
            if (Is64Bit)
                table_set_mixed_bool64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
            else
                table_set_mixed_bool32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ipValue);
        }





        [DllImport(L64, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_get_column_index64(QueryHandle queryPtr,
            [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen);

        [DllImport(L32, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_get_column_index32(QueryHandle queryPtr,
            [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen);

        public static long QueryGetColumnIndex(Query q, String columnName)
        {
            if (Is64Bit)
                return (long)query_get_column_index64(q.QueryHandle, columnName, (IntPtr)columnName.Length);
            return (long)query_get_column_index32(q.QueryHandle, columnName, (IntPtr)columnName.Length);
        }



        //in tightdb c++ this function returns q again, the query object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations query.dothis().dothat()
        [DllImport(L64, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_int_greater64(QueryHandle queryPtr, IntPtr columnIndex, long value);

        [DllImport(L32, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_int_greater32(QueryHandle queryPtr, IntPtr columnIndex, long value);

        public static void query_int_greater(Query q, long columnIndex, long value)
        {
            if (Is64Bit)
                query_int_greater64(q.QueryHandle, (IntPtr)columnIndex, value);
            else
                query_int_greater32(q.QueryHandle, (IntPtr)columnIndex, value);
        }


        //in tightdb c++ this function returns q again, the query object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations query.dothis().dothat()
        [DllImport(L64, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_bool_equal64(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(L32, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_bool_equal32(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        public static void QueryBoolEqual(Query q, long columnIndex, bool value)
        {
            var ipValue = BoolToIntPtr(value);
            if (Is64Bit)
                query_bool_equal64(q.QueryHandle, (IntPtr)columnIndex, ipValue);
            else
                query_bool_equal32(q.QueryHandle, (IntPtr)columnIndex, ipValue);
        }



        //in tightdb c++ this function returns q again, the query object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations query.dothis().dothat()
        [DllImport(L64, EntryPoint = "query_int_between", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_int_between64(QueryHandle queryPtr, IntPtr columnIndex, long lowValue,
            long highValue);

        [DllImport(L32, EntryPoint = "query_int_between", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_int_between32(QueryHandle queryPtr, IntPtr columnIndex, long lowValue,
            long highValue);

        public static void QueryIntBetween(Query q, long columnIndex, long lowValue, long highValue)
        {
            if (Is64Bit)
                query_int_between64(q.QueryHandle, (IntPtr)columnIndex, lowValue, highValue);
            else
                query_int_between32(q.QueryHandle, (IntPtr)columnIndex, lowValue, highValue);
        }


        [DllImport(L64, EntryPoint = "query_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_count64(QueryHandle queryPtr, IntPtr start, IntPtr end, IntPtr limit);

        [DllImport(L32, EntryPoint = "query_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_count32(QueryHandle queryPtr, IntPtr start, IntPtr end, IntPtr limit);

        public static long QueryCount(Query q, long start, long end, long limit)
        {
            return Is64Bit
                ? (long) query_count64(q.QueryHandle, (IntPtr) start, (IntPtr) end, (IntPtr) limit)
                : (long) query_count32(q.QueryHandle, (IntPtr) start, (IntPtr) end, (IntPtr) limit);
        }


        [DllImport(L64, EntryPoint = "tableview_set_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_double64(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            double value);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_double32(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            double value);

        public static void TableViewSetMixedDouble(TableView tableView, long columnIndex, long rowIndex, double value)
        {
            if (Is64Bit)
                tableview_set_mixed_double64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                tableview_set_mixed_double32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }


        [DllImport(L64, EntryPoint = "tableview_set_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_double64(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            double value);

        [DllImport(L32, EntryPoint = "tableview_set_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_double32(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            double value);

        public static void TableViewSetDouble(TableView tableView, long columnIndex, long rowIndex, double value)
        {
            if (Is64Bit)
                tableview_set_double64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                tableview_set_double32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }

        [DllImport(L64, EntryPoint = "table_set_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_double64(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            double value);

        [DllImport(L32, EntryPoint = "table_set_mixed_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_double32(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            double value);

        public static void TableSetMixedDouble(Table table, long columnIndex, long rowIndex, double value)
        {
            if (Is64Bit)
                table_set_mixed_double64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_set_mixed_double32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }


        [DllImport(L64, EntryPoint = "table_set_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_double64(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex, double value);

        [DllImport(L32, EntryPoint = "table_set_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_double32(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex, double value);

        public static void TableSetDouble(Table table, long columnIndex, long rowIndex, double value)
        {
            if (Is64Bit)
                table_set_double64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_set_double32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }








        [DllImport(L64, EntryPoint = "tableview_set_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_float64(TableViewHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            float value);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_mixed_float32(TableViewHandle  tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            float value);

        public static void TableViewSetMixedFloat(TableView tableView, long columnIndex, long rowIndex, float value)
        {
            if (Is64Bit)
                tableview_set_mixed_float64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                tableview_set_mixed_float32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }



        [DllImport(L64, EntryPoint = "table_set_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_float64(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            float value);

        [DllImport(L32, EntryPoint = "table_set_mixed_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_float32(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            float value);

        public static void TableSetMixedFloat(Table table, long columnIndex, long rowIndex, float value)
        {
            if (Is64Bit)
                table_set_mixed_float64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_set_mixed_float32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }



        [DllImport(L64, EntryPoint = "tableview_set_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_float64(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            float value);

        [DllImport(L32, EntryPoint = "tableview_set_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_float32(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            float value);

        public static void TableViewSetFloat(TableView tableView, long columnIndex, long rowIndex, float value)
        {
            if (Is64Bit)
                tableview_set_float64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                tableview_set_float32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }



        [DllImport(L64, EntryPoint = "table_set_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_float64(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex, float value);

        [DllImport(L32, EntryPoint = "table_set_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_float32(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex, float value);

        public static void TableSetFloat(Table table, long columnIndex, long rowIndex, float value)
        {
            if (Is64Bit)
                table_set_float64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
            else
                table_set_float32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value);
        }


        //setting and getting Date fields from the database

        //keeping this static might speed things up, instead of instantiating a new one every time
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        //convert a DateTime to a 64 bit integer to be marshalled to a time_t on the other side
        //NOTE THAT TIGHTDB ROUNDS DOWN TO NEAREST SECOND WHEN STORING A DATETIME
        //Note also that the date supplied is converted to UTC - we assume that the user has set the datetimekind.utc 

        //ALSO NOTE THAT TIGHTDB CANNOT STORE time_t values that are negative, effectively tighdb is not able to store dates before 1970,1,1
        public static Int64 ToTightDbDateTime(DateTime date)
        {
            return (Int64) (date.ToUniversalTime() - Epoch).TotalSeconds;
        }

        private static Int64 ToTightDbMixedDateTime(DateTime date)
        {
            var retval = ToTightDbDateTime(date);
            if (retval < 0)
            {
                throw new ArgumentOutOfRangeException("date",
                    "The date specified is not a valid tightdb mixed date. Tightdb mixed dates must be positive time_t dates, from jan.1.1970 and onwards ");
            }
            return retval;
        }

        //not used
        //CppTime is expected to be a time_t (UTC since 1970,1,1). While currently tightdb cannot handle negative time_t, this method will work wiht those just fine
        /*
        public static DateTime ToCSharpTimeLocalTime(Int64 cppTime)
        {            
            return  ToCSharpTimeUtc(cppTime).ToLocalTime();
        }
        */


        //CppTime is expected to be a time_t (UTC since 1970,1,1 measured in seconds)
        public static DateTime ToCSharpTimeUtc(Int64 cppTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(cppTime));
            //unfortunate that addseconds takes a double. Addseconds rounds the double is rounded to nearest millisecond so we should not have problems with loose precision
        }


        [DllImport(L64, EntryPoint = "table_set_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_date64(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            Int64 value);

        [DllImport(L32, EntryPoint = "table_set_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_mixed_date32(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex,
            Int64 value);

        public static void TableSetMixedDate(Table table, long columnIndex, long rowIndex, DateTime value)
        {
            if (Is64Bit)
                table_set_mixed_date64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex,
                    ToTightDbMixedDateTime(value));
            else
                table_set_mixed_date32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex,
                    ToTightDbMixedDateTime(value));
        }


        [DllImport(L64, EntryPoint = "tableview_set_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_mixed_date64(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            Int64 value);

        [DllImport(L32, EntryPoint = "tableview_set_mixed_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableView_set_mixed_date32(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            Int64 value);

        public static void TableViewSetMixedDate(TableView tableView, long columnIndex, long rowIndex, DateTime value)
        {
            if (Is64Bit)
                tableView_set_mixed_date64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex,
                    ToTightDbMixedDateTime(value));
            else
                tableView_set_mixed_date32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex,
                    ToTightDbMixedDateTime(value));
        }



        [DllImport(L64, EntryPoint = "table_set_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_date64(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex, Int64 value);

        [DllImport(L32, EntryPoint = "table_set_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_date32(TableHandle tablePtr, IntPtr columnIndex, IntPtr rowIndex, Int64 value);

        public static void TableSetDate(Table table, long columnIndex, long rowIndex, DateTime value)
        {
            if (Is64Bit)
                table_set_date64(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ToTightDbDateTime(value));
            else
                table_set_date32(table.TableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ToTightDbDateTime(value));
        }


        [DllImport(L64, EntryPoint = "tableview_set_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_date64(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            Int64 value);

        [DllImport(L32, EntryPoint = "tableview_set_date", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tableview_set_date32(TableViewHandle tableViewPtr, IntPtr columnIndex, IntPtr rowIndex,
            Int64 value);

        public static void TableViewSetDate(TableView tableView, long columnIndex, long rowIndex, DateTime value)
        {
            if (Is64Bit)
                tableview_set_date64(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ToTightDbDateTime(value));
            else
                tableview_set_date32(tableView.TableViewHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, ToTightDbDateTime(value));
        }

        //these methods are meant to be used for testing that the c++ library passes types on the stack of size, sequence and logical representation 
        //expected by the C# end.
        //the methods below are not used except in the test part of initialization
        //it looks like we are testing stuff that is defined to be set in stone, but especially the C# end could theoretically change, and it is not known until
        //we are running inside some CLR implementation. We also do not know the workings of the marshalling layer of the CLI we are running on, or the marshalling
        //layer of the C# compiler that built the C# source. Reg. the C++ compiler that built the c++ binding, we don't know for sure if it has bugs or quirks.
        //Because a problem would be extremely hard to track down, we make sure things work as we expect them to
        //if this method does not throw an exception, the types we use to communicate to/from c++ are safe and work as expected
        [DllImport(L64, EntryPoint = "test_sizeofsize_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int32 test_sizeofsize_t64();

        [DllImport(L32, EntryPoint = "test_sizeofsize_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int32 test_sizeofsize_t32();

        private static Int32 TestSizeOfSizeT()
        {
            if (Is64Bit)
                return test_sizeofsize_t64();
            return test_sizeofsize_t32();
        }

        [DllImport(L64, EntryPoint = "test_sizeofint32_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofint32_t64();

        [DllImport(L32, EntryPoint = "test_sizeofint32_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofint32_t32();

        private static IntPtr TestSizeOfInt32_T()
        {
            if (Is64Bit)
                return test_sizeofint32_t64();
            return test_sizeofint32_t32();
        }

        [DllImport(L64, EntryPoint = "test_sizeoftablepointer", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeoftablepointer64();

        [DllImport(L32, EntryPoint = "test_sizeoftablepointer", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeoftablepointer32();

        private static IntPtr TestSizeOfTablePointer()
        {
            if (Is64Bit)
                return test_sizeoftablepointer64();
            return test_sizeoftablepointer32();
        }

        [DllImport(L64, EntryPoint = "test_sizeofcharpointer", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofcharpointer64();

        [DllImport(L32, EntryPoint = "test_sizeofcharpointer", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofcharpointer32();

        private static IntPtr TestSizeOfCharPointer()
        {
            if (Is64Bit)
                return test_sizeofcharpointer64();
            return test_sizeofcharpointer32();
        }

        [DllImport(L64, EntryPoint = "test_sizeofint64_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofint64_t64();

        [DllImport(L32, EntryPoint = "test_sizeofint64_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofint64_t32();

        private static IntPtr Testsizeofint64_t()
        {
            if (Is64Bit)
                return test_sizeofint64_t64();
            return test_sizeofint64_t32();
        }



        [DllImport(L64, EntryPoint = "test_sizeoffloat", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeoffloat64();

        [DllImport(L32, EntryPoint = "test_sizeoffloat", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeoffloat32();

        private static IntPtr TestSizeOfFloat()
        {
            if (Is64Bit)
                return test_sizeoffloat64();
            return test_sizeoffloat32();
        }


        [DllImport(L64, EntryPoint = "test_sizeofdouble", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofdouble64();

        [DllImport(L32, EntryPoint = "test_sizeofdouble", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeofdouble32();

        private static IntPtr TestSizeOfDouble()
        {
            if (Is64Bit)
                return test_sizeofdouble64();
            return test_sizeofdouble32();
        }




        [DllImport(L64, EntryPoint = "test_sizeoftime_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeoftime_t64();

        [DllImport(L32, EntryPoint = "test_sizeoftime_t", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_sizeoftime_t32();

        private static IntPtr TestSizeOfTime_T()
        {
            if (Is64Bit)
                return test_sizeoftime_t64();
            return test_sizeoftime_t32();
        }



        [DllImport(L64, EntryPoint = "test_get_five_parametres", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_get_five_parametres64(IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5);

        [DllImport(L32, EntryPoint = "test_get_five_parametres", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_get_five_parametres32(IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5);

        private static IntPtr TestGetFiveParametres(IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, IntPtr p5)
        {
            if (Is64Bit)
                return test_get_five_parametres64(p1, p2, p3, p4, p5);
            return test_get_five_parametres32(p1, p2, p3, p4, p5);
        }



        [DllImport(L64, EntryPoint = "test_float_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern float test_float_max64();

        [DllImport(L32, EntryPoint = "test_float_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern float test_float_max32();

        private static float TestFloatMax()
        {
            if (Is64Bit)
                return test_float_max64();
            return test_float_max32();
        }

        [DllImport(L64, EntryPoint = "test_float_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern float test_float_min64();

        [DllImport(L32, EntryPoint = "test_float_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern float test_float_min32();

        private static float TestFloatMin()
        {
            if (Is64Bit)
                return test_float_min64();
            return test_float_min32();
        }


        [DllImport(L64, EntryPoint = "test_float_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern float test_float_return64(float value);

        [DllImport(L32, EntryPoint = "test_float_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern float test_float_return32(float value);

        private static float TestFloatReturn(float value)
        {
            if (Is64Bit)
                return test_float_return64(value);
            return test_float_return32(value);
        }



        [DllImport(L64, EntryPoint = "test_double_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern double test_double_max64();

        [DllImport(L32, EntryPoint = "test_double_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern double test_double_max32();

        private static double TestDoubleMax()
        {
            if (Is64Bit)
                return test_double_max64();
            return test_double_max32();
        }

        [DllImport(L64, EntryPoint = "test_double_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern double test_double_min64();

        [DllImport(L32, EntryPoint = "test_double_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern double test_double_min32();

        private static double TestDoubleMin()
        {
            if (Is64Bit)
                return test_double_min64();
            return test_double_min32();
        }


        [DllImport(L64, EntryPoint = "test_double_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern double test_double_return64(double value);

        [DllImport(L32, EntryPoint = "test_double_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern double test_double_return32(double value);

        private static double TestDoubleReturn(double value)
        {
            if (Is64Bit)
                return test_double_return64(value);
            return test_double_return32(value);
        }





        [DllImport(L64, EntryPoint = "test_int64_t_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_int64_t_max64();

        [DllImport(L32, EntryPoint = "test_int64_t_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_int64_t_max32();

        private static long TestLongMax()
        {
            if (Is64Bit)
                return test_int64_t_max64();
            return test_int64_t_max32();
        }

        [DllImport(L64, EntryPoint = "test_int64_t_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_int64_t_min64();

        [DllImport(L32, EntryPoint = "test_int64_t_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_int64_t_min32();

        private static long TestLongMin()
        {
            if (Is64Bit)
                return test_int64_t_min64();
            return test_int64_t_min32();
        }


        [DllImport(L64, EntryPoint = "test_int64_t_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_int64_t_return64(long value);

        [DllImport(L32, EntryPoint = "test_int64_t_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_int64_t_return32(long value);

        private static long TestLongReturn(long value)
        {
            if (Is64Bit)
                return test_int64_t_return64(value);
            return test_int64_t_return32(value);
        }








        [DllImport(L64, EntryPoint = "test_size_t_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_t_max64();

        [DllImport(L32, EntryPoint = "test_size_t_max", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_t_max32();

        private static IntPtr TestSizeTMax()
        {
            if (Is64Bit)
                return test_size_t_max64();
            return test_size_t_max32();
        }

        [DllImport(L64, EntryPoint = "test_size_t_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_t_min64();

        [DllImport(L32, EntryPoint = "test_size_t_min", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_t_min32();

        private static IntPtr TestSizeTMin()
        {
            if (Is64Bit)
                return test_size_t_min64();
            return test_size_t_min32();
        }


        [DllImport(L64, EntryPoint = "test_size_t_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_t_return64(IntPtr value);

        [DllImport(L32, EntryPoint = "test_size_t_return", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_size_t_return32(IntPtr value);

        private static IntPtr TestSizeTReturn(IntPtr value)
        {
            if (Is64Bit)
                return test_size_t_return64(value);
            return test_size_t_return32(value);
        }


        [DllImport(L64, EntryPoint = "test_return_datatype", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_datatype64(IntPtr value);

        [DllImport(L32, EntryPoint = "test_return_datatype", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_datatype32(IntPtr value);

        private static DataType TestReturnDataType(DataType value)
        {
            if (Is64Bit)
                return IntPtrToDataType(test_return_datatype64(DataTypeToIntPtr(value)));
            return IntPtrToDataType(test_return_datatype32(DataTypeToIntPtr(value)));
        }


        [DllImport(L64, EntryPoint = "test_return_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_bool64(IntPtr value);

        [DllImport(L32, EntryPoint = "test_return_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_bool32(IntPtr value);

        private static Boolean TestReturnBoolean(Boolean value)
        {
            if (Is64Bit)
                return IntPtrToBool(test_return_bool64(BoolToIntPtr(value)));
            return IntPtrToBool(test_return_bool32(BoolToIntPtr(value)));
        }

        [DllImport(L64, EntryPoint = "test_return_true_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_true_bool64();

        [DllImport(L32, EntryPoint = "test_return_true_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_true_bool32();

        private static Boolean TestReturnTrueBool()
        {
            if (Is64Bit)
                return IntPtrToBool(test_return_true_bool64());
            return IntPtrToBool(test_return_true_bool32());
        }



        [DllImport(L64, EntryPoint = "test_return_false_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_false_bool64();

        [DllImport(L32, EntryPoint = "test_return_false_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_return_false_bool32();

        private static Boolean TestReturnFalseBool()
        {
            if (Is64Bit)
                return IntPtrToBool(test_return_false_bool64());
            return IntPtrToBool(test_return_false_bool32());
        }

        /*depricated - was used for testing purposes
        [DllImport(L64, EntryPoint = "test_increment_integer", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_increment_integer64(long value);
        [DllImport(L32, EntryPoint = "test_increment_integer", CallingConvention = CallingConvention.Cdecl)]
        private static extern long test_increment_integer32(long value);

        private static long TestIncrementInteger(long value)//warning okay - was used for test purposes earlier. may be deleted entirely soon
        {
            if (Is64Bit)
                return test_increment_integer64(value);
            return test_increment_integer32(value);
        }
        */



        //I am not sure wether the string will be pinned and c will get a pointer to its buffer
        //or if the string wil be copied to somewhere and then a pointer to the copy is passed
        //it is also unclear wether the string will only be served to c++ up to its first null character
        //or if the entire string is available.
        //As these things are barely documented, we have to test and probably handle different platforms differently

        [DllImport(L64, EntryPoint = "test_string_to_cpp", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_to_cpp_64([MarshalAs(UnmanagedType.LPWStr)] String s, IntPtr len);

        [DllImport(L32, EntryPoint = "test_string_to_cpp", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_to_cpp_32([MarshalAs(UnmanagedType.LPWStr)] String s, IntPtr len);

        private static long TestStringToCpp(String str)
        {
            if (Is64Bit)
                return (long) test_string_to_cpp_64(str, (IntPtr) str.Length);
            return (long) test_string_to_cpp_32(str, (IntPtr) str.Length);
        }


        //this test method will round-trip a string into a c++ StringData and back again to c++
        //great for testing that all string conversion is working
        [DllImport(L64, EntryPoint = "test_string_returner", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_returner64([MarshalAs(UnmanagedType.LPWStr)] String s, IntPtr slength,
            IntPtr b, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "test_string_returner", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_returner32([MarshalAs(UnmanagedType.LPWStr)] String s, IntPtr slength,
            IntPtr b, IntPtr bufsize);

        private static string TestStringReturner(String str)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;

            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long) test_string_returner64(str, (IntPtr) str.Length, buffer, (IntPtr) currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (long) test_string_returner32(str, (IntPtr) str.Length, buffer, (IntPtr) currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));

            return StrBufToStr(buffer, (int) bufferSizeNeededChars);

        }

        //okay no coverage of the error case, should only fail if broken on the platform
        private static void ReturnStringTest(string stringtotest)
        {
            string returnedstring = TestStringReturner(stringtotest);
            if (returnedstring != stringtotest)
            {
                throw new ArgumentOutOfRangeException("stringtotest", String.Format(CultureInfo.InvariantCulture,
                    "Sent {0} to cpp but got {1} back", stringtotest, returnedstring));
            }
        }



        //a very simple "do we have some kind of connection test that returns the string "Hello, World!" from c++ to c#
        //more demanding unit tests will be done elsewhere - the purpose of this test is to make sure the most basic stuff works (8/16 bits chars etc)
        //this example uses Marshal to provide an unmanaged piece of memory for c++. Eventually data is copied back into a managed string.
        //this method is used as a template for copying wherever we return a string 
        [DllImport(L64, EntryPoint = "test_string_from_cpp", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_from_cpp_ptr64(IntPtr b, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "test_string_from_cpp", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_from_cpp_ptr32(IntPtr b, IntPtr bufsize);

        private static string TestStringFromCppUsingPointers()
        {

            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;

            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (Is64Bit)
                    bufferSizeNeededChars = (long) test_string_from_cpp_ptr64(buffer, (IntPtr) currentBufferSizeChars);
                else
                    bufferSizeNeededChars = (long) test_string_from_cpp_ptr32(buffer, (IntPtr) currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));

            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }

        //todo:performance test this, to figure if we really need these annotations
        //after the call currentBufferSizeChars is the size of the buffer. Before the call, bufferSizeNeededChars holds the requested size
        //while currentbuffersizeChars holds the size from last time the buffer was created
#if V45PLUS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        private static IntPtr StrAllocateBuffer(out long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            currentBufferSizeChars = bufferSizeNeededChars;
            return Marshal.AllocHGlobal((IntPtr) (bufferSizeNeededChars*sizeof (char)));
            //allocHGlobal instead of  AllocCoTaskMem because allcHGlobal allows lt 2 gig on 64 bit (not that .net supports that right now, but at least this allocation will work with lt 32 bit strings)   
        }

        //uses the marshaller to copy a unicode utf-16string inside an umnanaged buffer into a C# string
        //after the copy operation, the buffer is released using Marshal.FreeHGlobal(allocated in strallocatebuffer just above with Marshal.AllcHGlobal)
#if V45PLUS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        private static string StrBufToStr(IntPtr buffer, int bufferSizeNeededChars)
        {
            string retStr = bufferSizeNeededChars > 0 ? Marshal.PtrToStringUni(buffer, bufferSizeNeededChars) : "";
            //return "" if the string is empty, otherwise copy data from the buffer
            Marshal.FreeHGlobal(buffer);
            return retStr;
        }

        //determines if the buffer was large enough by looking at requested size and current size. if not large enough, free the buffer and return
        //true which makes the calling method loop once more
#if V45PLUS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        private static Boolean StrBufferOverflow(IntPtr buffer, long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            if (currentBufferSizeChars < bufferSizeNeededChars)
            {
                Marshal.FreeHGlobal(buffer);

                return true;
            }
            return false;
        }


        /*

        //a very simple "do we have some kind of StringBuilder connection test that returns the string "Hello, World!" from c++ to c#
        //more demanding unit tests will be done elsewhere - the purpose of this test is to make sure the most basic stuff works (8/16 bits chars etc)
        //note that this string return example uses stringbuilder, and that stringbuilder will marshal such that the first null in the string, terminates the string
        //so this C#-c++ platform ivocation string transfer works only with strings without null characters inside (null terminated ones are okay)
        //the c++ side provides us with a bufszneed value that indicates the actual length of the string returned
        [DllImport(L64, EntryPoint = "test_string_from_cpp", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_from_cpp64([MarshalAs(UnmanagedType.LPWStr)]StringBuilder b, IntPtr bufsize);
        [DllImport(L32, EntryPoint = "test_string_from_cpp", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr test_string_from_cpp32([MarshalAs(UnmanagedType.LPWStr)]StringBuilder b, IntPtr bufsize);

        public static string TestStringFromCppUsingStringBuilder()
        {
            var b = new StringBuilder(16);

            bool loop = true;
            do
            {
                long bufszneed;//in 16 bit chars
                if (Is64Bit)
                    bufszneed =
                        (long)test_string_from_cpp64( b, (IntPtr)b.Capacity);
                else
                    bufszneed =
                        (long)test_string_from_cpp32(b, (IntPtr)b.Capacity);

                if (b.Capacity < bufszneed)
                //Capacity is in .net chars, each of size 16 bits, bufszneed is also in 16 bit chars
                {
                    //what we know for sure is that the stringbuilder will hold AT LEAST capacity number of bytes. The c++ dll counts in bytes, so there is always room enough.
                    if (bufszneed + 1 <= b.MaxCapacity)
                        b.Capacity = (int)bufszneed + 1;
                    else
                    {
                        throw new ArgumentOutOfRangeException("columnIndex", String.Format(CultureInfo.InvariantCulture, "trying to recieve a string from tightdb, but the string length in 16 bit characters ({0}) is larger than the maximum string length supported by stringbuilder in .net ({1})", bufszneed, b.MaxCapacity));
                    }
                    //not 100% sure the +1 is neccessary, but by providing the extra 1 char, we allow the c++  end to null terminate without getting in trouble                    
                }
                else{
                    loop = false;
                    b.Length = (int)bufszneed;
                }
            } while (loop);
            return b.ToString();
            //in c# this does NOT result in a copy, we get a string that points to the B buffer (If the now immutable string inside b is reused , it will get itself a new buffer
        }
        */



        [DllImport(L64, EntryPoint = "table_optimize", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_optimize64(TableHandle handle);

        [DllImport(L32, EntryPoint = "table_optimize", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_optimize32(TableHandle handle);

        public static void TableOptimize(Table table)
        {
            if (Is64Bit)
                table_optimize64(table.TableHandle);
            else
                table_optimize32(table.TableHandle);
        }



        [DllImport(L64, EntryPoint = "table_to_json", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_to_json64(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "table_to_json", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_to_json32(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize);


        public static string TableToJson(Table t)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (int)table_to_json64(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (int)table_to_json32(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }


        [DllImport(L64, EntryPoint = "tableview_get_source_ndx", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_source_ndx64(TableViewHandle handle, IntPtr rowNdx);

        [DllImport(L32, EntryPoint = "tableview_get_source_ndx", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tableview_get_source_ndx32(TableViewHandle handle, IntPtr rowNdx);

        public static long TableViewGetSourceIndex(TableView tableview, long rowIndex)
        {
            return Is64Bit
                ? (long) tableview_get_source_ndx64(tableview.TableViewHandle, (IntPtr) rowIndex)
                : (long) tableview_get_source_ndx32(tableview.TableViewHandle, (IntPtr) rowIndex);
        }


        [DllImport(L64, EntryPoint = "tableview_to_json", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_to_json64(TableViewHandle tableHandle, IntPtr buffer, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "tableview_to_json", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_to_json32(TableViewHandle tableHandle, IntPtr buffer, IntPtr bufsize);

        public static string TableViewToJson(TableView t)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long) tableview_to_json64(t.TableViewHandle, buffer, (IntPtr) currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (long)tableview_to_json32(t.TableViewHandle, buffer, (IntPtr)currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }






        [DllImport(L64, EntryPoint = "table_to_string_defaultlimit", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_to_string_defaultlimit64(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize);

        [DllImport(L32, EntryPoint = "table_to_string_defaultlimit", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_to_string_defaultlimit32(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize);


        public static string TableToString(Table t)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (int)table_to_string_defaultlimit64(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (int)table_to_string_defaultlimit32(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars);
            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }


        [DllImport(L64, EntryPoint = "tableview_to_string_defaultlimit", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_to_string_defaultlimit64(TableViewHandle tableHandle, IntPtr buffer,
            IntPtr bufsize);

        [DllImport(L32, EntryPoint = "tableview_to_string_defaultlimit", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_to_string_defaultlimit32(TableViewHandle tableHandle, IntPtr buffer,
            IntPtr bufsize);

        public static string TableViewToString(TableView t)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)tableview_to_string_defaultlimit64(t.TableViewHandle, buffer, (IntPtr)currentBufferSizeChars);
                else
                    bufferSizeNeededChars =
                        (long)tableview_to_string_defaultlimit32(t.TableViewHandle, buffer, (IntPtr)currentBufferSizeChars);
            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }



        [DllImport(L64, EntryPoint = "table_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_to_string64(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize, IntPtr limit);

        [DllImport(L32, EntryPoint = "table_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_to_string32(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize, IntPtr limit);


        public static string TableToString(Table t, long limit)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (int)table_to_string64(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars, (IntPtr)limit);
                else
                    bufferSizeNeededChars =
                        (int)table_to_string32(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars, (IntPtr)limit);
            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }


        [DllImport(L64, EntryPoint = "tableview_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_to_string64(TableViewHandle tableViewHandle, IntPtr buffer, IntPtr bufsize,
            IntPtr limit);

        [DllImport(L32, EntryPoint = "tableview_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_to_string32(TableViewHandle tableViewHandle, IntPtr buffer, IntPtr bufsize,
            IntPtr limit);

        public static string TableViewToString(TableView t, long limit)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)tableview_to_string64(t.TableViewHandle, buffer, (IntPtr)currentBufferSizeChars, (IntPtr)limit);
                else
                    bufferSizeNeededChars =
                        (long)tableview_to_string32(t.TableViewHandle, buffer, (IntPtr)currentBufferSizeChars, (IntPtr)limit);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }










        [DllImport(L64, EntryPoint = "table_row_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_row_to_string64(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize,
            IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "table_row_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr table_row_to_string32(TableHandle tableHandle, IntPtr buffer, IntPtr bufsize,
            IntPtr rowIndex);


        public static string TableRowToString(Table t, long rowIndex)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (int)
                            table_row_to_string64(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars, (IntPtr)rowIndex);
                else
                    bufferSizeNeededChars =
                        (int)
                            table_row_to_string32(t.TableHandle, buffer, (IntPtr)currentBufferSizeChars, (IntPtr)rowIndex);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }


        [DllImport(L64, EntryPoint = "tableview_row_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_row_to_string64(TableViewHandle  tableHandle, IntPtr buffer, IntPtr bufsize,
            IntPtr rowIndex);

        [DllImport(L32, EntryPoint = "tableview_row_to_string", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr tableview_row_to_string32(TableViewHandle tableHandle, IntPtr buffer, IntPtr bufsize,
            IntPtr rowIndex);

        public static string TableViewRowToString(TableView t, long rowIndex)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;
            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                if (Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                            tableview_row_to_string64(t.TableViewHandle, buffer, (IntPtr)currentBufferSizeChars,
                                (IntPtr) rowIndex);
                else
                    bufferSizeNeededChars =
                        (long)
                            tableview_row_to_string32(t.TableViewHandle, buffer, (IntPtr)currentBufferSizeChars,
                                (IntPtr) rowIndex);
            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }








        //this can be called when initializing, and it will throw an exception if anything is wrong with the type binding between the  C#
        //program running right now, and the c++ library that has been loaded.
        //The test should reveal if any of the types we use are marshalled wrongly, especially
        //size on the stack
        //min and max values match
        //parametres are sent in the right sequence
        //logical meaning of the basic types we use

        //for instance this test would discover if a time_t on the c++ side is 32 bit, while the C# side expects 64 bits.
        //The test will be valuable if any of these things change :
        //C# compiler, CLR implementation, c++ compiler that built the c++ part, operating system

        //the current code, when built with .net visual studio is expected to work on these CLR (Common Language Runtime)implementations :
        //windows 32 bit
        //windows 64 bit
        //support for these are partial (some unit tests still fail)
        //mono windows 32 bit
        //mono windows 64 bit
        //support for these are coming up (stil untested, some c++ work needed)
        //mono linux x86    (selected distributions)
        //mono linux x86-64 (selected distributions)

        //the current C# code is expected to work on these C# compilers
        //windows C# target x64
        //windows C# target x32
        //windows C# target anycpu
        //support for these are coming up, but they do build.
        //mono C# compiler target x64
        //mono C# compiler target x32
        //mono C# compiler target AnyCpu
        //mono C# compiler other targets (SPARC, PowerPC,S390 64 bit... )

        //the current C# and c++ code is expected and tested to work on these operating systems (system platforms)
        //windows 7 32 bit (using .net, compiled with VS2012 C# and VS2012 c++)
        //windows 7 64 bit (using .net, compiled with VS2012 C# and VS2012 c++)

        //the following platforms will be supported when needed
        //windows phone 
        //silverlight 
        //ps3 (using mono/linux)
        //android (using mono/linux)

        //there are many more combinations. To enable a combination to work, the following must be done
        //1) ensure tightdb and the c++ part of the binding can be built on the platform
        //2) ensure there is a CLR on the platform
        //3) any needed changes to the C# part of the binding must be implemented (could be calling conventions, type sizes, quirks with marshalling, names of library files to load, etc)
        //4) optional - ensure that the platform C# compiler can build the C# part of tightdb 
        //  (if the C# platform is not binary compatible with .net we have to build on the specific C# platform, if it is binary compatible this is optional)


        private static void TestInteropSizes()
        {
            var sizeOfIntPtr = IntPtr.Size;
            var sizeOfSizeT = TestSizeOfSizeT();
            if (sizeOfIntPtr != sizeOfSizeT)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The size_t size{0} does not match the size of IntPtr{1}", sizeOfSizeT, sizeOfIntPtr));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The size_t size{0} does not match the size of IntPtr{1}", sizeOfSizeT, sizeOfIntPtr));
#endif
            }

            IntPtr sizeOfInt32T = TestSizeOfInt32_T();
            var sizeOfInt32 = (IntPtr) sizeof (Int32);
            if (sizeOfInt32T != sizeOfInt32)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The int32_t size{0} does not match the size of Int32{1}", sizeOfInt32T, sizeOfInt32));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The int32_t size{0} does not match the size of Int32{1}", sizeOfInt32T, sizeOfInt32));
#endif
            }
            //from here on, we know that size_t maps fine to IntPtr, and Int32_t maps fine to Int32

            IntPtr sizeOfTablePointer = TestSizeOfTablePointer();
            var sizeOfIntPtrAsIntPtr = (IntPtr) IntPtr.Size;
            if (sizeOfTablePointer != sizeOfIntPtrAsIntPtr)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The Table* size{0} does not match the size of IntPtr{1}", sizeOfTablePointer, sizeOfIntPtrAsIntPtr));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The Table* size{0} does not match the size of IntPtr{1}", sizeOfTablePointer, sizeOfIntPtrAsIntPtr));
#endif
            }

            IntPtr sizeOfCharPointer = TestSizeOfCharPointer();
            if (sizeOfCharPointer != sizeOfIntPtrAsIntPtr)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The Char* size{0} does not match the size of IntPtr{1}", sizeOfCharPointer, sizeOfIntPtrAsIntPtr));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The Char* size{0} does not match the size of IntPtr{1}", sizeOfCharPointer, sizeOfIntPtrAsIntPtr));
#endif
            }

            IntPtr sizeOfInt64T = Testsizeofint64_t();
            var sizeOfLong = (IntPtr) sizeof (long);

            if (sizeOfInt64T != sizeOfLong)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The Int64_t size{0} does not match the size of long{1}", sizeOfInt64T, sizeOfLong));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The Int64_t size{0} does not match the size of long{1}", sizeOfInt64T, sizeOfLong));
#endif
            }


            IntPtr sizeOfTimeT = TestSizeOfTime_T();
            var sizeOfTimeTReceiverType = (IntPtr) sizeof (Int64);
            //we put time_t into an Int64 before we convert it to DateTime - we expect the time_t to be of type 64 bit integer

            if (sizeOfTimeT != sizeOfTimeTReceiverType)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The c++ time_t size({0}) does not match the size of the C# recieving type int64 ({1})", sizeOfTimeT,
                    sizeOfTimeTReceiverType));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The c++ time_t size({0}) does not match the size of the C# recieving type int64 ({1})", sizeOfTimeT,
                    sizeOfTimeTReceiverType));
#endif
            }

            IntPtr sizeOffloatPlus = TestSizeOfFloat();
            var sizeOfFloatSharp = (IntPtr) sizeof (float);

            if (sizeOffloatPlus != sizeOfFloatSharp)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The c++ float size{0} does not match the size of C# float{1}", sizeOffloatPlus, sizeOfFloatSharp));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The c++ float size{0} does not match the size of C# float{1}", sizeOffloatPlus, sizeOfFloatSharp));
#endif
            }

            var sizeOfPlusDouble = (long) TestSizeOfDouble();
            const long sizeOfSharpDouble = sizeof (double);

            if (sizeOfPlusDouble != sizeOfSharpDouble)
            {
#if V40PLUS
                throw new ContextMarshalException(String.Format(CultureInfo.InvariantCulture,
                    "The c++ double size({0}) does not match the size of the C# recieving type Double ({1})",
                    sizeOfPlusDouble, sizeOfSharpDouble));
#else
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "The c++ double size({0}) does not match the size of the C# recieving type Double ({1})",
                    sizeOfPlusDouble, sizeOfSharpDouble));
#endif
            }



        }

        //It is okay that code coverage is low for this method - the exceptions will only trigger if 
        //we have been linked to a c++ compiler that does not meet our expectations reg. the C ABI        
        private static void TestMaxMin()
        {
            float floatmaxcpp = TestFloatMax();
            const float floatmaxcs = float.MaxValue;
            if (floatmaxcpp > floatmaxcs || floatmaxcpp < floatmaxcs)
            {
                throw new ArgumentException("float",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ max float value seems to be {0:r} while the C# is {1:r}", floatmaxcpp, floatmaxcs));
            }

            //current documentation on c# Sigle/float http://msdn.microsoft.com/en-us/library/system.single.aspx
            float floatmincpp = TestFloatMin();
            const float floatmincs = float.MinValue;
            if (floatmincpp > floatmincs || floatmincpp < floatmincs)
            {
                throw new ArgumentException("float",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ min float value seems to be {0:r} while the C# is {1:r}", floatmincpp, floatmincs));
            }

            const float float42 = 42f;
            float cppfortytwo = TestFloatReturn(float42);
            if (float42 > cppfortytwo || float42 < cppfortytwo)
            {
                throw new ArgumentException("float",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ float 42f was returned as {0:r} while the C# value is {1:r}", float42, cppfortytwo));
            }




            double doublemaxcpp = TestDoubleMax();
            const double doublemaxcs = double.MaxValue;
            if (doublemaxcpp > doublemaxcs || doublemaxcpp < doublemaxcs)
            {
                throw new ArgumentException("double",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ max double value seems to be {0:r} while the C# is {1:r}", doublemaxcpp, doublemaxcs));
            }


            double doublemincpp = TestDoubleMin();
            const double doublemincs = double.MinValue;
            if (doublemincpp > doublemincs || doublemincpp < doublemincs)
            {
                throw new ArgumentException("double",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ min double value seems to be {0:r} while the C# is {1:r}", doublemincpp, doublemincs));
            }

            const double double42 = 42f;
            double cppfortytwodouble = TestDoubleReturn(double42);
            if (double42 > cppfortytwodouble || double42 < cppfortytwodouble)
            {
                throw new ArgumentException("double",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ double 42f was returned as {0:r} while the C# value is {1:r}", double42,
                        cppfortytwodouble));
            }




            //very well defined in C#. Should also be so in c++ so this should never fail  http://msdn.microsoft.com/en-us/library/ctetwysk.aspx
            long longmaxcpp = TestLongMax();
            const long longmaxcs = long.MaxValue;
            if (longmaxcpp != longmaxcs)
            {
                throw new ArgumentException("long",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ max int64_t (mapped to long) value seems to be {0:r} while the C# long is {1:r}",
                        longmaxcpp, longmaxcs));
            }


            long longmincpp = TestLongMin();
            const long longmincs = long.MinValue;
            if (longmincpp != longmincs)
            {
                throw new ArgumentException("long",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ min int64_t (mapped to long) value seems to be {0:r} while the C# is {1:r}", longmincpp,
                        longmincs));
            }

            const long long42 = 42;
            long cppfortytwolong = TestLongReturn(long42);
            if (long42 != cppfortytwolong)
            {
                throw new ArgumentException("long",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ int64_t 42 (mapped to long) was returned as {0:r} while the C# value is {1:r}", long42,
                        cppfortytwolong));
            }





            IntPtr sizeTMaxCpp = TestSizeTMax();
#if V40PLUS
            IntPtr sizeTMaxCs = IntPtr.Zero;
            sizeTMaxCs = sizeTMaxCs - 1;
#else
    // 3.5 and below do not support adding or subtracting to IntPtr
    //so we simply set this variable so that this test always succeeds
            var sizeTMaxCs = sizeTMaxCpp;
#endif
            if (sizeTMaxCpp != sizeTMaxCs)
            {
                throw new ArgumentException("size_t",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ max size_t value seems to be {0:r} while the C# is {1:r}", sizeTMaxCpp, sizeTMaxCs));
            }




            IntPtr sizeTMinCpp = TestSizeTMin();
            IntPtr sizeTMinCs = IntPtr.Zero;
            if (sizeTMinCpp != sizeTMinCs)
            {
                throw new ArgumentException("size_t",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ min size-t value seems to be {0:r} while the C# is {1:r}", sizeTMinCpp, sizeTMinCs));
            }

            var sizeTCs = (IntPtr) 42;
            var sizeTCpp = TestSizeTReturn(sizeTCs);
            if (sizeTCpp != sizeTCs)
            {
                throw new ArgumentException("Size_t",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ size_t 42 was returned as {0:r} while the C# value is {1:r}", sizeTCpp, sizeTCs));
            }


        }




        //this dll call is only used in ShowVersionTest. 
        //will probably not work with mono so at that time should be inside a define
        //as should the line below using it
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [PreserveSig]
        private static extern uint GetModuleFileName
            (
            [In] IntPtr hModule,
            [Out] StringBuilder lpFilename,
            [In] [MarshalAs(UnmanagedType.U4)] int nSize
            );



        //This could become a global const as it should only be set once
        //we are a dll and as such do not have any code that is called on initialization (there is no initialization in a .net assembly)
        //however we could call a common init method from the few methods that the user can decide to start out with. Such as new table, new group etc.
        //the call would be called many times but could just return at once, if we were already initialized
        //and if we were not, we could set the is64bit boolean correctly - and then all other calls (does not call init) could just check that boolean
        //would speed up interop call overhead about 3-4 percent, currently at about 100 million a second on a 2.6GHZ cpu
        private static bool Is64Bit
        {
            get
            {
                return (IntPtr.Size == 8);
                //if this is evaluated every time, a faster way could be implemented. Size is cost when we are running though so perhaps it gets inlined by the JITter
            }
        }

        private static bool BuiltWithMono
        {
            get
            {
#if (__MonoCS__)
                return true;
#else
                return false;
#endif
            }
        }

#if (DEBUG)
        private const string Buildmode = "d";
        private const string BuildName = "Debug";
#else
        private const string Buildmode = "r";
        private const string BuildName = "Release";

#endif




        //the .net library wil always use a c dll that is called tightdb_c_cs2012[32/64][r/d]
        //this dll could have been built with vs2012 or 2010 - we don't really care as long as the C interface is the same, which it will be
        //if built from the same source.

        private const String L64 = "tightdb_c_cs201264" + Buildmode;
        private const String L32 = "tightdb_c_cs201232" + Buildmode;


        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        //todo:in case we can't load our dll, we should throw an exception with the location that we would
        //prefer to find the dll. (assembly location) and the name of the dll we failed to load

        //todo:an enhanced version of this, will show all the search paths that .net will use to get
        //at the dll.



        private static string Dllstring()
        {
            return (IntPtr.Size == 8) ? L64 : L32;
        }

        //returns info that can be gathered without actually loading the c++ dll
        public static String GetCsInfo()
        {
            var info = new StringBuilder();
            var pointerSize = IntPtr.Size;
            var vmBitness = (pointerSize == 8) ? "64bit" : "32bit";
#if V40PLUS

            var is64BitOs = Environment.Is64BitOperatingSystem ? "Yes" : "No";
            var is64BitProcess = Environment.Is64BitProcess;
#else
            const string is64BitOs = "Unknown in .net below 4.0";
            const string is64BitProcess = "Unknown in .net below 4.0";
#endif

            var compiledWithMono = BuiltWithMono ? "Yes" : "No";

            OperatingSystem os = Environment.OSVersion;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            PortableExecutableKinds peKind;
            ImageFileMachine machine;
            executingAssembly.ManifestModule.GetPEKind(out peKind, out machine);
            var thisAppLocation = executingAssembly.Location;

            info.AppendLine("");
            const string fmt = "{0,-30}:{1,-15}";
            info.AppendLine("---OS Info---");
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "OS Version", os.Version));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "OS Platform", os.Platform));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "64 Bit OS", is64BitOs));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "64 Bit process", is64BitProcess));
            info.AppendLine("---OS Info---");

            info.AppendLine("");
            info.AppendLine("---CLR Info---");
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Pointer Size", pointerSize));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Process Running as", vmBitness));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Running on mono", IsRunningOnMono()));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Common Language Runtime",
                Environment.Version));
            info.AppendLine("---CLR Info---");

            info.AppendLine("");
            info.AppendLine("---C# binding (TightDbCSharp.dll) Info---");
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Built as PeKind", peKind));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Built as ImageFileMachine", machine));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Debug Or Release", BuildName));
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Compiled With Mono", compiledWithMono));
#if V45PLUS
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, fmt, "Built for .net version", "V4.5"));
#elif V40PLUS
            info.AppendLine(String.Format(CultureInfo.InvariantCulture,fmt,"Built for .net version","V4.0"));
#else
            info.AppendLine(String.Format(CultureInfo.InvariantCulture,fmt,"Built for .net version","V3.5"));
#endif
            info.AppendLine("---C# binding (TightDbCSharp.dll) Info---");

            info.AppendLine("");
            info.AppendLine("---C++ DLL Info---");
            info.AppendLine("Assembly running right now :");
            info.AppendLine(thisAppLocation);
            info.AppendLine("Current Directory :");
            info.AppendLine(Directory.GetCurrentDirectory());
            //info.AppendLine("");
            info.AppendLine(String.Format(CultureInfo.InvariantCulture, "Now Loading {0} - expecting it to be a {1} dll",
                Dllstring(), vmBitness));
            return info.ToString();
        }

        public static string GetCppInfo()
        {
            var info = new StringBuilder();
            //most exceptions while loading the dll first time will abort the program and cannot be caught
            //but try to catch anything that might be catchable
            try
            {
                var cppDllVersion = CppDllVersion();
                const int maxPath = 260;
                var builder = new StringBuilder(maxPath);
                var hModule = GetModuleHandle(Dllstring());
                uint hresult = 0;
                if (hModule != IntPtr.Zero)
                    //could be zero if the dll has never been called, but then we would not be here in the first place
                {
                    hresult = GetModuleFileName(hModule, builder, builder.Capacity);
                }
                if (hresult != 0)
                {
                    info.AppendLine("");
                    info.AppendLine(String.Format(CultureInfo.InvariantCulture, "DLL File Actually Loaded \n:{0}",
                        builder));
                }
                info.AppendLine(String.Format(CultureInfo.InvariantCulture, "C#  DLL        build number {0}",
                    Toolbox.GetDllVersionCSharp));
                info.AppendLine(String.Format(CultureInfo.InvariantCulture, "C++ DLL        build number {0}",
                    cppDllVersion));
                info.AppendLine("---C++ DLL Info---");

                info.AppendLine();
                info.AppendLine();
            }

            catch (Exception e)
            {
//mono might crash if we get here, as it does not support c++ thrown exceptions
                info.AppendLine(String.Format(CultureInfo.InvariantCulture,
                    "Exception thrown while attempting to call c++ dll {0}", e.Message));
            }
            return info.ToString();
        }




        //if something is wrong with interop or C# marshalling, this method will throw an exception
        //low unit test coverage is okay, the error cases will only be hit if the platform behaves very unexpected
        public static void TestInterop()
        {

            //that was the sizes of the basic types. Now check if parametres are sent and recieved in the correct sequence


            TestInteropSizes();

            IntPtr testres = TestGetFiveParametres((IntPtr) 1, (IntPtr) 2, (IntPtr) 3, (IntPtr) 4, (IntPtr) 5);
            if ((long) testres != 1)
            {
                throw new ArgumentException("parameter sequence",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ parameter sequence appears to be broken. called test_get_five_parametres(1,2,3,4,5) expected 1 as a return value, got {0}",
                        testres));
            }


            TestMaxMin();

            const DataType test = DataType.Binary;
            var test2 = TestReturnDataType(test);
            if (test != test2)
            {
                throw new ArgumentException("DataType",
                    String.Format(CultureInfo.InvariantCulture,
                        "The c++ returned Datatype  {0} is not the same as was sent by c# {1}", test2, test));
            }


            if (TestReturnFalseBool())
            {
                throw new ArgumentException("c++ TestReturnFalseBool returned true");
            }

            if (!TestReturnTrueBool())
            {
                throw new ArgumentException("c++ TestReturnTrueBool returned false");
            }

            if (!TestReturnBoolean(true))
            {
                throw new ArgumentException("sent true to testreturnboolean, got false back");
            }

            if (TestReturnBoolean(false))
            {
                throw new ArgumentException("sent false to testreturnboolean, got true back");
            }


            //strings

            //testing calling and receiving a string, no tightdb involved
            var resultstring2 = TestStringFromCppUsingPointers();
            if (!resultstring2.Equals("Hello, World!"))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                    "cpp returned Hello, World! but C# recieved{0}", resultstring2));
            }

            //testing calling c++ with a string parameter. method returns 42 if the parameter is "Hello, World!"

            const string testtext = "Hello, World!";
            if (42 != TestStringToCpp(testtext))
            {
                throw new ArgumentException(
                    "Test string sent to cpp didn't show up as expected. Cpp did not return 42 when sent 'Hello, World!' ");
            }


            ReturnStringTest("Hello, World!"); //Just an ordinary string
            ReturnStringTest("Is 15 chars long"); //small enough to not trigger roundtrip code
            ReturnStringTest("Is 16 chars long!"); //exactly the bordercase for buffer overflow
            ReturnStringTest("Is 16+1 chars long"); //overflows 16 char buffer and demands a new call
            ReturnStringTest(""); //an empty string
            ReturnStringTest("κόσμε"); //unicode sample that would not work with ANSI
            ReturnStringTest("To be Done"); //unicode sample that will involve two 16 bit chars to encode one codepoint
            ReturnStringTest("This is so long that it does not fit inside 48 bytes when converted to UTF-8");
            //Quite long string that overruns the 48 byte preallocated buffer in cpp to c, and forces a real calculation of the length before UTF16 to UTF conversion
            ReturnStringTest("This string has a null here:\x0000And more data afterwards");
            //null characters inside a string should be preserved
            ReturnStringTest("This string ends with a null characer and has a null here:\x0000The End\x0000");
            //null characters inside a string should be preserved, also if they are the last character in the string
            ReturnStringTest("拉亂𠀀蠟螺𠀁嵐æøå珞藍𠀂𠀃foo𠀄洛𠀅𠀆邏𠀇𠀈𠀉𠀊欄𠀋𠀌𠀍蘭𠀎𠀏羅");
        }


        //shared group implementation


        [DllImport(L64, EntryPoint = "new_shared_group_file_defaults", CallingConvention = CallingConvention.Cdecl)]
        private static extern SharedGroupHandle new_shared_group_file_defaults64(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);

        [DllImport(L32, EntryPoint = "new_shared_group_file_defaults", CallingConvention = CallingConvention.Cdecl)]
        private static extern SharedGroupHandle new_shared_group_file_defaults32(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);

        //the interop will return a SharedGroupHandle where root is null.
        //in this case, that is quite okay as a shared group is in fact a root
        //otherwise we would have to set root this method returned
        public static void NewSharedGroupFileDefaults(SharedGroup sharedGroup, string filename)
        {
            sharedGroup.SetHandle(Is64Bit
                ? new_shared_group_file_defaults64(filename, (IntPtr) filename.Length)
                : new_shared_group_file_defaults32(filename, (IntPtr) filename.Length), false);
                //shared groups are not readonly as a default
        }


        [DllImport(L64, EntryPoint = "new_shared_group_file", CallingConvention = CallingConvention.Cdecl)]
        private static extern SharedGroupHandle new_shared_group_file64(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen, IntPtr noCreate, IntPtr durabilityLevel);

        [DllImport(L32, EntryPoint = "new_shared_group_file", CallingConvention = CallingConvention.Cdecl)]
        private static extern SharedGroupHandle new_shared_group_file32(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen, IntPtr noCreate, IntPtr durabilityLevel);


        public static void NewSharedGroupFile(SharedGroup group, string fileName, bool noCreate,
            DurabilityLevel durabilityLevel)
        {
            SharedGroupHandle handle = Is64Bit
                ? new_shared_group_file64(fileName, (IntPtr) fileName.Length, BoolToIntPtr(noCreate),
                    SharedGroup.DurabilityLevelToIntPtr(durabilityLevel))
                : new_shared_group_file32(fileName, (IntPtr) fileName.Length, BoolToIntPtr(noCreate),
                    SharedGroup.DurabilityLevelToIntPtr(durabilityLevel));
            if (handle.IsInvalid)
                //todo:This test checks for zero or minus one as invalid. Do more elaborate error codes,especially for most common kinds of IO errors
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "New SharedGroup failed filename {0} probably due to an IO error in core", fileName));
            }
            group.SetHandle(handle, false);
        }



        [DllImport(L64, EntryPoint = "shared_group_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void shared_group_delete64(IntPtr handle);

        [DllImport(L32, EntryPoint = "shared_group_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void shared_group_delete32(IntPtr handle);

        public static void SharedGroupDelete(IntPtr sharedGroupHandle)
        {
            if (Is64Bit)
                shared_group_delete64(sharedGroupHandle);
            else
                shared_group_delete32(sharedGroupHandle);

            // sharedGroup.Handle = IntPtr.Zero;
        }

        /* depricated
        [DllImport(L64, EntryPoint = "shared_group_open", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_open64(IntPtr sharedGroupHandle, [MarshalAs(UnmanagedType.LPWStr)]  string fileName, IntPtr fileNameLen, IntPtr noCreate, IntPtr durabilityLevel);

        [DllImport(L32, EntryPoint = "shared_group_open", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_open32(IntPtr sharedGroupHandle, [MarshalAs(UnmanagedType.LPWStr)] string fileName, IntPtr fileNameLen, IntPtr noCreate, IntPtr durabilityLevel);

        
        public static void SharedGroupOpen(SharedGroup group, string fileName, bool noCreate, DurabilityLevel durabilityLevel)
        {
            try
            {
                if (Is64Bit) 
                    shared_group_open64(group.Handle,fileName, (IntPtr)fileName.Length, BoolToIntPtr(noCreate),
                        SharedGroup.DurabilityLevelToIntPtr(durabilityLevel));

                    shared_group_open32(group.Handle,fileName, (IntPtr)fileName.Length, BoolToIntPtr(noCreate),
                        SharedGroup.DurabilityLevelToIntPtr(durabilityLevel));
            }
            catch (SEHException ex)
            {
                throw new IOException(
                    String.Format(
                        "IO error creating or reading group file {0} (read/write access is needed)  c++ exception thrown :{1}",
                        fileName, ex.Message));
            }
        }
        */

        /*
        [DllImport(L64, EntryPoint = "shared_group_is_attached", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_is_attached64(IntPtr handle);

        [DllImport(L32, EntryPoint = "shared_group_is_attached", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_is_attached32(IntPtr handle);

        public static Boolean SharedGroupIsAttached(SharedGroup sharedGroup)
        {
            if (Is64Bit)
                return IntPtrToBool(shared_group_is_attached64(sharedGroup.Handle));
            return IntPtrToBool(shared_group_is_attached32(sharedGroup.Handle));            
        }

        */



        [DllImport(L64, EntryPoint = "shared_group_reserve", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_reserve64(SharedGroupHandle handle, IntPtr bytesToReserve);

        [DllImport(L32, EntryPoint = "shared_group_reserve", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_reserve32(SharedGroupHandle handle, IntPtr bytesToReserve);

        public static void SharedGroupReserve(SharedGroup sharedGroup, long bytesToReserve)
        {
            var errorcode =
                (Is64Bit)
                    ? (long) shared_group_reserve64(sharedGroup.SharedGroupHandle, (IntPtr) bytesToReserve)
                    : //possible overflow on 32 bit reg .bytesToReserve
                    (long) shared_group_reserve32(sharedGroup.SharedGroupHandle, (IntPtr) bytesToReserve);

            if (errorcode < 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Call to Shared Group reserve failed. Error code {0}", errorcode));
            }
        }



        [DllImport(L64, EntryPoint = "shared_group_has_changed", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_has_changed64(SharedGroupHandle handle);

        [DllImport(L32, EntryPoint = "shared_group_has_changed", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_has_changed32(SharedGroupHandle handle);

        public static Boolean SharedGroupHasChanged(SharedGroup sharedGroup)
        {
            return IntPtrToBool(Is64Bit
                ? shared_group_has_changed64(sharedGroup.SharedGroupHandle)
                : shared_group_has_changed32(sharedGroup.SharedGroupHandle)
                );
        }


        //this is complicated.
        //The call to shared_group_begin_read must result in us always having two things inside a sharedgroup
        //handle : the shared group pointer, AND the shared group transaction state set to InReadTransaction
        //

        [DllImport(L64, EntryPoint = "shared_group_begin_read", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_begin_read64(SharedGroupHandle sharedGroupPtr);

        [DllImport(L32, EntryPoint = "shared_group_begin_read", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_begin_read32(SharedGroupHandle sharedGroupPtr);


        //used by sharedgrouphandle to start transactions
        public static IntPtr SharedGroupBeginRead(SharedGroupHandle sharedGroupHandle)
        {
            return Is64Bit ? shared_group_begin_read64(sharedGroupHandle) : shared_group_begin_read32(sharedGroupHandle);
            //sharedGrup.Handle used bc we call shardgroup to doa beginread
        }

        [DllImport(L64, EntryPoint = "shared_group_begin_write", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_begin_write64(SharedGroupHandle sharedGroupPtr);

        [DllImport(L32, EntryPoint = "shared_group_begin_write", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_begin_write32(SharedGroupHandle sharedGroupPtr);



        //used by sharedgrouphandle to start writetransactions. Group handle is returned
        public static IntPtr SharedGroupBeginWrite(SharedGroupHandle sharedGroupHandle)
        {
            if (Is64Bit)
                return shared_group_begin_write64(sharedGroupHandle);
            //sharedGrup.Handle used bc we call shardgroup to doa beginread
            return shared_group_begin_write32(sharedGroupHandle);
        }




        [DllImport(L64, EntryPoint = "shared_group_commit", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_commit64(SharedGroupHandle sharedGroupHandle);

        [DllImport(L32, EntryPoint = "shared_group_commit", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_commit32(SharedGroupHandle sharedGroupHandle);


        //called by SharedGroupHandle atomically as an ongoing transaction is a sort of leak protected resource
        public static IntPtr SharedGroupCommit(SharedGroupHandle sharedGroupHandle)
        {
            return (Is64Bit)
                ? shared_group_commit64(sharedGroupHandle)
                : shared_group_commit32(sharedGroupHandle);
        }




        //todo:add return value to rollback if c++ threw an exception
        [DllImport(L64, EntryPoint = "shared_group_rollback", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_rollback64(SharedGroupHandle handle);

        [DllImport(L32, EntryPoint = "shared_group_rollback", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_rollback32(SharedGroupHandle handle);


        //called by SharedGroupHandle atomically
        public static IntPtr SharedGroupRollback(SharedGroupHandle sharedGroupHandle)
        {
            return (Is64Bit)
                ? shared_group_rollback64(sharedGroupHandle)
                : shared_group_rollback32(sharedGroupHandle);
        }



        [DllImport(L64, EntryPoint = "shared_group_end_read", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_end_read64(SharedGroupHandle handle);

        [DllImport(L32, EntryPoint = "shared_group_end_read", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_end_read32(SharedGroupHandle handle);

        //called by SharedGroupHandle atomically to set state correctly
        public static IntPtr SharedGroupEndRead(SharedGroupHandle sharedgroupHandle)
        {
            return (Is64Bit)
                ? shared_group_end_read64(sharedgroupHandle)
                : shared_group_end_read32(sharedgroupHandle);
        }
    }
}