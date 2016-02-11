# About

This sample ThreeLayerRealmXF was created in VS2015 using _Xamarin Forms Portable_ template. It was later updated to use Xamarin Forms 2.0 which involved no code changes, just selecting a higher version in NuGet.

Diving into giving PCL layering a full workout, it represents the typical approach of a user who uses Xamarin Forms in a PCL for their portable GUI as well having a separate ViewModel class in another PCL assembly.

## The Layers

* Platform-specific app such as `ThreeLayerRealmXF.IOS` or `ThreeLayerRealmXF.Droid`.
* `ThreeLayerXF` UI Layer of common Xamarin Forms UI.
* `PurePCLViewModel` Sandwich filling invoked by `ThreeLayerXF`.
* `Realm.PCL` layer which invokes Realm via the _Bait and Switch_ pattern.

## Build Notes

`PurePCLViewModel` was created using the VS2015 template _Class Library (Portable)_ with default targets to match the Xamarin Forms PCL default:

* .Net Framework 4.5
* Windows 8
* Windows Phone Silverlight 8
* Windows Phone 8.1
* Xamarin.Android
* Xamarin.iOS
* Xamarin.iOS (Classic)

## Minimal NuGet

Rather than using NuGet to supply `Realm` and `RealmWeaver`, we directly include the relevant DLL files directly from the adjacent build directories, assuming this sample lives under the `test` dir of the main `realm-dotnet` repo.

NuGet is used just to supply Fody, Xamarin Forms and some Android libs.

## Direct Realm Library Inclusion

To match the `Realm.PCL` dll used by `PurePCLViewModel` we include specific DLLs per platform:

* in `ThreeLayerRealmXF.iOS.csproj` for Platform `iPhone` include `Realm.XamarinIOS\bin\iPhone\Release\Realm.dll`
* in `ThreeLayerRealmXF.iOS.csproj` for Platform `iPhoneSimulator` include `Realm.XamarinIOS\bin\iPhoneSimulator\Release\Realm.dll`
* in `ThreeLayerRealmXF.Droid.csproj` include `Realm.Realm.XamarinAndroid\bin\Release\Realm.dll`
* in `ThreeLayerRealmXF.Droid.csproj` include three lots of `..\..\..\..\wrappers\build\Release-android\armeabi\libwrappers.so` which specify an explicit `<Abi>` element 

## Other Notes

We also have the `RealmWeaver.Fody` project included so as to provide easier debugging of Fody weaving errors. (Search the `ModuleWeaver.cs` file for `Debugger.Launch()`).

`ThreeLayerRealmXF.WinPhone` is not yet completed as we lack a Realm build for WinPhone
