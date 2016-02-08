# Notes on our PCL Implementation and Testbeds

## Realm.PCL


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

