# Notes on our PCL Implementation and Testbeds

## Realm.PCL

This needs to mimic the **public** API of Shared.Realm.

We want to ensure the same namespace and classes are available through here but `GetInstance` throws an exception prompting you to directly link libs to your executable.

Basically, everything that appears in the public [API Class List](https://realm.io/docs/xamarin/latest/api/annotated.html)

If you use NuGet, that should happen automatically.

### Proxy files in Realm.PCL

The following provide gutted stubs as proxies.

* RealmConfigurationPCL.cs
* RealmListPCL.cs
* RealmObjectPCL.cs
* RealmPCL.cs
* TransactionPCL.cs

Note that we link to the Exception classes and include them verbatim as they are already very lightweight.

## ThreeLayerXFRealm
This is a sample program that mimics the user's 
Created in VS2015 using _Xamarin Forms Portable_ template

Diving into giving PCL layering a full workout.

* `ThreeLayerXFRealm` UI Layer of common Xamarin Forms UI
* `PurePCLViewModel` Sandwich filling invoked by `ThreeLayerXF`
* `Realm.PCL` layer 

`PurePCLViewModel` and `NativeCallingBottom` created using the VS2015 template _Class Library (Portable)_ with default targets to match the Xamarin Forms PCL default:

* .Net Framework 4.5
* Windows 8
* Windows Phone Silverlight 8
* Windows Phone 8.1
* Xamarin.Android
* Xamarin.iOS
* Xamarin.iOS (Classic)

