Java vs C# Mapping Layers
====================

The Java layer has calls to Checked or Unchecked versions of functions in the JNI.

This is invoked by 
`CheckedRow.java`

```java
public long getLong(long columnIndex) {
	return nativeGetLong(nativePointer, columnIndex);
}

// with the definition later in the file of
protected native long nativeGetLong(long nativeRowPtr, long columnIndex);


```

This calls either of the following depending on whether you start with a `CheckedRow` or `UncheckedRow` object, as both define the native calls, effectively as polymorphic dispatch.

```cpp
JNIEXPORT jlong JNICALL Java_io_realm_internal_CheckedRow_nativeGetLong
  (JNIEnv* env, jobject obj, jlong nativeRowPtr, jlong columnIndex)
{
    if (!ROW_AND_COL_INDEX_AND_TYPE_VALID(env, ROW(nativeRowPtr), columnIndex, type_Int))
        return 0;

    return Java_io_realm_internal_UncheckedRow_nativeGetLong(env, obj, nativeRowPtr, columnIndex);
}


JNIEXPORT jlong JNICALL Java_io_realm_internal_UncheckedRow_nativeGetLong
  (JNIEnv* env, jobject, jlong nativeRowPtr, jlong columnIndex)
{
    TR_ENTER_PTR(nativeRowPtr)
    if (!ROW_VALID(env, ROW(nativeRowPtr)))
        return 0;

    return ROW(nativeRowPtr)->get_int( S(columnIndex) );
}    
```

