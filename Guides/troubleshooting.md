# Troubleshooting - .NET SDK
## Resolve a 'No Properties in Class' Exception
You may see a `System.InvalidOperationException` with the message
`"No properties in class, has linker stripped it?"`.

There are three known causes of this exception:

- You have no woven `RealmObjects`, probably because something went wrong
with Fody. If this is the case, `RealmSchema` throws an exception. See
Fody Weave Exceptions section for details on how to fix this.
- A linker has stripped properties from one or more `RealmObjects`, and Realm thinks
those properties don't exist. This can occur if you have your
[Linker Behavior](https://docs.microsoft.com/en-us/xamarin/ios/deploy-test/linker?tabs=macos)
set to `Link all assemblies` but have not used the `[Preserve(AllMembers = true)]`
attribute on the class declaration. The linker only preserves class members
that you have referenced explicitly in the code or marked with the
`[Preserve(AllMembers = true)]` attribute. This means that if you want to
persist a property in realm and it is not referenced anywhere
in your code, the linker may remove it, causing a schema mismatch.
- You are using a code obfuscation tool which is interfering with
model name detection. Realm relies on class names to generate the schema, so
if a code obfuscation tool has hidden those class names, schema generation
fails. To solve this, set your code obfuscation tool to ignore your model classes.

## Fody: 'An Unhandled Exception Occurred'
This common build failure is triggered when you have already built a
project and then add a new `RealmObject` sub-class. Choosing to
Build or Run your project does not rebuild the project
thoroughly enough to invoke the Fody Weaver. To fix this,
choose to **Rebuild your project**.

## Fody Weave Exceptions
You may see a warning in the build log about classes not having been woven. This
indicates that the Fody weaving package is not properly installed. Here are some
things to check:

1. Check that the `FodyWeavers.xml` file contains an entry for Realm.
2. It is also possible that the installation of Fody has failed. Users have
experienced this with Visual Studio 2015 and versions of NuGet Package Manager
prior to version 3.2. To diagnose this, use a text editor to check that your
`.csproj` has a line importing
Fody.targets`, such as:
```xml
<Import Project="..\packages\Fody.1.29.3\build\portable-net+sl+win+wpa+wp\Fody.targets"
 Condition="Exists('..\packages\Fody.1.29.3\build\portable-net+sl+win+wpa+wp\Fody.targets')" />
 ```
The easiest fix is to upgrade to a later version of NuGet Package Manager. If
this doesn't work, there may be a problem with Fody and `Microsoft.Bcl.Build.targets`.
Removing the following line from your .csproj file might help:

```xml
<Import Project="..\..\packages\Microsoft.Bcl.Build.1.0.21\build\Microsoft.Bcl.Build.targets"
 Condition="Exists('..\..\packages\Microsoft.Bcl.Build.1.0.21\build\Microsoft.Bcl.Build.targets')" />
```

## Troubleshooting WriteAsync Issues
Inside the `WriteAsync()` method, we check for a non-null
`SynchronizationContext.Current` object to determine if the code executes on
the UI thread. This check may be inaccurate if you have set `Current` in your
worker threads. If you have set `SynchronizationContext.Current` in a
worker thread, call the `Write()` method instead of `WriteAsync()`.

For more information, see Asynchronous Writes.

## Resolving a 'AddDefaultTypes should be called before creating a Realm instance' Exception
You may see a `System.NotSupportedException` with the message "AddDefaultTypes
should be called before creating a Realm instance". This can happen when you have
multiple projects containing Realm models and you open a Realm instance before
all of them have been loaded in memory.

### Explanation of the Cause
.NET uses lazy loading to improve performance. An assembly is not loaded until
one or more of the types it contains is needed. However, Realm relies on the
[module initializer](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/module-initializers)
to register all of the types in its default schema. When you first open a realm,
the default schema gets locked, and any attempt to register more types will result
in the exception above.

### Resolution
There are 3 ways to resolve this issue:

1. You can place your realm models in the same assembly where you call `Realm.GetInstance`.
2. You can pre-load the assembly that contains your realm objects by creating a
static method on the assembly, and then call that method from your app
code:
```csharp
// In your models assembly
public static class MyAssemblyLoader
{
    public static void LoadMe() {}
}
// In your app
public void Initialize()
{
    MyAssemblyLoader.LoadMe();
}
```
3. You can explicitly specify the Realm schema when initializing the
   realm:
```csharp
var config = new RealmConfiguration
{
    Schema = new[] { typeof(Person), typeof(Company), ... }
}
```

## iOS/iPad OS Bad Alloc/Not Enough Memory Available
In iOS or iPad devices with little available memory, or where you have a
memory-intensive application that uses multiple realms or many notifications,
you may encounter the following error:

```console
libc++abi: terminating due to an uncaught exception of type std::bad_alloc: std::bad_alloc
```

This error typically indicates that a resource cannot be allocated because
not enough memory is available.

If you are building for iOS 15+ or iPad 15+, you can add the
[Extended Virtual Addressing Entitlement](https://developer.apple.com/documentation/bundleresources/entitlements/com_apple_developer_kernel_extended-virtual-addressing)
to resolve this issue.

Add these keys to your Property List, and set the values to `true`:

```xml
<key>com.apple.developer.kernel.extended-virtual-addressing</key>
<true/>
<key>com.apple.developer.kernel.increased-memory-limit</key>
<true/>
```
