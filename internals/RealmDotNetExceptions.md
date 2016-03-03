Exceptions
==============

The problem with catching exceptions, if you have any need to map specific types, is how you invoke the mapping code. A macro such as the `CATCH_STD` you see mentioned below can be used to provide a `catch` and then invoke a forwarding function which further processes the exception.

We use the pattern introduced in the 2015 hackathon Rust binding is to wrap the function invocation in a lambda so it can be nested in a `handle_errors` function to provide the wrapping of the lambda in a `try...catch` with further processing. That imposes (a tiny?) overhead on every successful call.

C# Overview
-----------------
The C# implementation described below is based on the Java exceptions but with limitations based on the platform. The essence of the difference is that the JNI layer can throw exceptions direct to Java from C++ but C# needs a delegate calling from C++ to throw the exceptions in _managed space._

**Warning** this pattern doesn't work in pure Windows (see issue 385) with it reporting _“An exception of the type…occurred in Realm.dll and wasn’t handled before a managed/native boundary"_

@dot
digraph { 
  node[shape = box]
  edge[arrowhead=vee]
  
/* c++ */
node [fontcolor="blue", color="blue"]
set_exception_thrower
ManagedExceptionThrower[shape=invhouse]
coreException [label="a std::exception\n thrown from core"]
throw_exception
hande_errors [label="hande_errors lambda\nwrapping all calls to core"]
convert_exception
/*  we don't do this now but maybe should "Binding invalid\n parameter detection"-> throw_exception */


  /*  C# */
node [fontcolor="orange", color="orange"]
SetupExceptionThrower [label="NativeCommon\n.SetupExceptionThrower"]

"CoreProvider\nconstructor" -> SetupExceptionThrower
ExceptionThrower -> "Switch on numeric code\nthrowing range of c# exceptions"

SetupExceptionThrower -> set_exception_thrower [label="passes pointer to delegate\n NativeCommon.ExceptionThrower"]

set_exception_thrower -> ManagedExceptionThrower [label="sets static\n function pointer"]

coreException ->  hande_errors -> convert_exception -> throw_exception 
throw_exception -> ManagedExceptionThrower [label="Rethrows to catch,\nconverting to code.\nThen callback C# with\n code for switch"]
ManagedExceptionThrower -> ExceptionThrower [label="points to\n managed function"]
ExceptionThrower  -> "Exception should be\n caught by user code" [style=dotted]
}

@enddot



Callback Overview
-------------------------

* The C# side declares a delegate which can receive an integer error code and optional string (having a string defined by length > 0)
* A single call is made to set a callback function in the Native DLL to call an instance of this delegate, 
	* the delegate callback will then be used whenever we want to throw exceptions from native. 
	* inside the delegate callback, we would typically switch on the error code to throw different kinds of exceptions
* Our interactions with native code are wrapped in try/catch

Callback Details - Native side
---------------------
We define a typedef for a callback function taking three parameters and a static var to keep a reference to it, publishing a function to set that callback:

```
using ManagedExceptionThrowerT = void(*)(size_t exceptionCode, void* utf8Str, size_t strLen);
static ManagedExceptionThrowerT ManagedExceptionThrower = nullptr;

extern "C" void set_exception_thrower(ManagedExceptionThrowerT userThrower)
{
    ManagedExceptionThrower = userThrower;
}
```

That function can then be invoked from any of our other native functions which have been invoked from the managed environmnent. Typically, it is invoked through the `handle_errors` lambda.

We assume a `std::string` will typically be generated containing a formatted error message in outfit format. 

```
template <class F>
auto handle_errors(F&& func) -> decltype(func())
{
    using RetVal = decltype(func());
    try {
        return func();
    }
    catch (...) {
        convert_exception();
        return Default<RetVal>::default_value();
    }
}
```

Callback Details - C# side
---------------------

We define a callback delegate type with a simple implementation like:

```
    public delegate void ExceptionThrowerCallback (IntPtr exceptionCode, IntPtr utf8String, IntPtr stringLen);


        [MonoPInvokeCallback (typeof (ExceptionThrowerCallback))]
        unsafe public static void ExceptionThrower(IntPtr exceptionCode, IntPtr utf8String, IntPtr stringLen)
        {
            //TODO in the real version this switch will throw different exceptions
            if ((Int64)stringLen > 0) {
                throw new SampleNativeException((Int64)exceptionCode, 
                    new String((sbyte*)utf8String, 0 /* start offset */, (int)stringLen, Encoding.UTF8));
            }
            throw new SampleNativeException((Int64)exceptionCode);
        }
```

This delegate is passed to our native side with a function that just needs invoking once, say from our `App` constructor.

```
    [DllImport(EXCEPTIONS_LIB)]
    public static extern void set_exception_thrower(ExceptionThrowerCallback callback);
    
    public static void SetupExceptionThrower()
    {
        set_exception_thrower (ExceptionThrower);
    }
```



Java Exceptions
---------------------------

Java is able to throw exceptions back through the JNI interface which it does through the `ThrowException` function

To get to that point, it usually  maps exceptions through a very lightweight macro  `CATCH_STD`  invoking  `ConvertException` . As seen in the diagram below, this rethrows the exception so it can catch different types.

A few other exceptions are caught by `CATCH_FILE`  which includes a range of catch clauses and thus directly calls `ThrowExcaption`.

Note that although `ThrowException` has a case for the enum `RuntimeError`, it is only used once. The chain through `CATCH_STD` invoking `ConvertException` maps all the `std::RuntimeError` through its default handling of `std::Exception`.

@dot
digraph { 
  node[shape = box]
  edge[arrowhead=vee]
  
/* c++ */
node [fontcolor="blue", color="blue"]
coreException [label="a std::exception\n thrown from core"]
CATCH_FILE
CATCH_STD

"Binding invalid\n parameter detection"-> ThrowException
coreExcep ion -> CATCH_FILE -> ThrowException
coreException -> CATCH_STD -> ConvertException

ConvertException -> ConvertException [label=" Rethrows exception"]
ConvertException -> ThrowException [label=" after catching\n rethrown exception"]
ThrowException -> "JNIEnv::FindException" [label=" lookup Java\n exception class"]
ThrowException -> "JNIEnv::ThrowNew" 

  /*  Java */
node [fontcolor="orange", color="orange"]
"JNIEnv::FindException" -> "Namespaced Java\n Exceptions"
"JNIEnv::ThrowNew" -> "Exception should be\n caught by user code" [style=dotted]
}

@enddot

