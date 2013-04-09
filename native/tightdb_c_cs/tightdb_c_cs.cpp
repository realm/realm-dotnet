// tightdb_c_cs.cpp : Defines the exported functions for the DLL application.
/*
    


*/

#include "stdafx.hpp"
#include "tightdb_c_cs.hpp"


using namespace tightdb;


#ifdef __cplusplus
extern "C" {
#endif


// return a (manually changed) constant - used when debugging to manually ensure a newly compiled dll is being linked to


 TIGHTDB_C_CS_API size_t tightdb_c_cs_GetVersion(void){

  // Table test;
	return 1304091004;
}
	
	

TIGHTDB_C_CS_API size_t table_get_column_count(tightdb::Table* TablePtr)
{
	return TablePtr->get_column_count();
}


//return a newly constructed top level table 
TIGHTDB_C_CS_API Table* new_table()
{
	//return reinterpret_cast<size_t>(LangBindHelper::new_table());	 
	return LangBindHelper::new_table();
}

TIGHTDB_C_CS_API void unbind_table_ref(tightdb::Table* TablePtr)
{
	//LangBindHelper::unbind_table_ref(reinterpret_cast<Table*>(TablePtr));
	LangBindHelper::unbind_table_ref(TablePtr);
}


//    size_t add_column(DataType type, const char* name, ColumnType attr=col_attr_None);
//note that we have omitted support for attr until we figure what it's for
TIGHTDB_C_CS_API size_t spec_add_column(Spec* SpecPtr,size_t type, const char* name) 
{
	return SpecPtr->add_column((DataType)type,name);		
}

//returns the spec that is associated with a table
//this spec is just a handle to use for spec operations and it does not need to be
//unbound or disposed of, it is the address of a spec that is managed by its table
TIGHTDB_C_CS_API Spec* table_get_spec(Table* TablePtr)
{
	//Table* t = reinterpret_cast<Table*>(TablePtr);
	Spec& s = TablePtr->get_spec();
	Spec* SpecPtr  = &s;
	return SpecPtr;
}



//    DataType    get_column_type(size_t column_ndx) const TIGHTDB_NOEXCEPT;
TIGHTDB_C_CS_API  DataType table_get_column_type(Table* TablePtr, const size_t column_ndx)
{
	return TablePtr->get_column_type(column_ndx);
}


TIGHTDB_C_CS_API  DataType spec_get_column_type(Spec* SpecPtr, const size_t column_ndx)
{
	return SpecPtr->get_column_type(column_ndx);
}




//#1 if the string (incl. nulll termination) does not fill in bufsize
//this function returns the desired bufsize needed to hold the string
//also, the buffer might have been updated with data,so the buffer contents is undefined
//in reality as much data as could fit in the buffer has been copied over
//however, the buffer is guarenteed not to have been overrun reg. bufsize
//#2 if the string did fit (including null termination is smaller or eq to bufsize)
//then sbuffer has been filled with the string, and the return value is the size of the string
//null termination excluded
//in general speed is dependent on the size of the src string, not on the size of the buffer
//as it would if strncpy had been used. Also, we do not create a buffer overrun as if strcpy had
//been used, and the string is too small compared to the buffer
//be aware that this code will likely change as soon as tightdb string representation changes
//also note that this function is used for column names as well as column values, so in case tightdb
//strings don't change for column names, but for column string data, two methods are needed, one for  char*
//and one for whatever new stuff we get in tightdb
size_t BSD_strlcpy(char * dst,size_t siz,const char * src)
{
	
	//strlcpy is not supported on many platforms, thus we use the BSD open source version,
	//without the function header, as we need that functionality (safe copy to buffer, size known)
    //the function has been changed to return and take size_t instead of int

	//to comply with copyright, the header below must be included.

/*	$OpenBSD: strlcpy.c,v 1.11 2006/05/05 15:27:38 millert Exp $	*/
/*
 * Copyright (c) 1998 Todd C. Miller <Todd.Miller@courtesan.com>
 *
 * Permission to use, copy, modify, and distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */


/*
 * Copy src to string dst of size siz.  At most siz-1 characters
 * will be copied.  Always NUL terminates (unless siz == 0).
 * Returns strlen(src); if retval >= siz, truncation occurred.
 */
//size_t
//strlcpy(char *dst, const char *src, size_t siz)
//{
	char *d = dst;
	const char *s = src;
	size_t n = siz;

	/* Copy as many bytes as will fit */
	if (n != 0) {
		while (--n != 0) {
			if ((*d++ = *s++) == '\0')
				break;
		}
	}

	/* Not enough room in dst, add NUL and traverse rest of src */
	if (n == 0) {
		if (siz != 0)
			*d = '\0';		/* NUL-terminate dst */
		while (*s++)
			;
	}

	return(s - src - 1);	/* count does not include NUL */
}


//bufsize is the c# capacity that a stringbuilder was created with
//colname is a buffer of at least that size
//the function should copy the column name into colname and zero terminate it, and return the number
//of bytes copied.
//in case the string does not fit inside bufsize, the method simply returns a value that is larger than
//bufsize - this is a request to be passed a buffer of larger size
//however, the buffer might have been filled up with as much data that it could hold acc. to bufsize

//this function will not make buffer overruns even if the column name is longer than the buffer passed
//c# is responsible for memory management of the buffer

TIGHTDB_C_CS_API int table_get_column_name(Table* TablePtr,size_t column_ndx,char * colname, size_t bufsize)
{
	const char* cn= TablePtr->get_column_name(column_ndx);
	return BSD_strlcpy(colname,bufsize, cn);
}


TIGHTDB_C_CS_API int spec_get_column_name(Spec* SpecPtr,size_t column_ndx,char * colname, size_t bufsize)
{
	const char* cn= SpecPtr->get_column_name(column_ndx);
	return BSD_strlcpy(colname,bufsize, cn);
}



//    Spec add_subtable_column(const char* name);
TIGHTDB_C_CS_API Spec* spec_add_subtable_column(Spec* SpecPtr, const char* name)
{
	//Spec* s = reinterpret_cast<Spec*>(SpecPtr);
	Spec subtablespec = SpecPtr->add_subtable_column(name);//will add_subtable_column return the address to a spec?
	return new Spec(subtablespec);
	
	//if I understand things correctly, SpecReturn will now BE the spec returned from add_subtable_column Spec IS the address of this class	
	//return reinterpret_cast<size_t>(Ret);//are we returning the address of the spec object returned by add_subtable_column?
}

//deallocate a spec that was allocated in this dll with new
TIGHTDB_C_CS_API void spec_deallocate(Spec* SpecPtr)
{
	delete(SpecPtr);
}

//FIXME: Should we check here on the c++ side, that column_ix is a subtable column before calling
//FIXME: Should this spec be deallocated? or is it part of the table structure it comes from? Currently the C# call is set not to call something similar to unbind_table_ref
TIGHTDB_C_CS_API Spec* spec_get_spec(Spec* SpecPtr,size_t column_ix)
{
    Spec subtablespec = SpecPtr->get_subtable_spec(column_ix);
    return new Spec(subtablespec);//will be unbound later on
}



TIGHTDB_C_CS_API size_t spec_get_column_count(Spec* SpecPtr)
{
    return SpecPtr->get_column_count();
}




#ifdef __cplusplus
}
#endif

/*
// This is the constructor of a class that has been exported.
// see TightCSDLL.h for the class definition
CTightCSDLL::CTightCSDLL()
{
	return;
}
*/

