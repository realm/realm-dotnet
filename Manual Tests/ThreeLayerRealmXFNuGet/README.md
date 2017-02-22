# About

Disclaimer: this experiment acts as a testbed for multiple layers of PCL and​_some_ databinding but should not be taken as an example of preferred architecture. It shows what is _technically possible_​rather than what is ​_good_​.

The solution ThreeLayerRealmXFNuGet was created in VS2015 using _Xamarin Forms Portable_ template. It was later updated to use Xamarin Forms 2.0 which involved no code changes, just selecting a higher version in NuGet.

Diving into giving PCL layering a full workout, it represents the typical approach of a user who uses Xamarin Forms in a PCL for their portable GUI as well having a separate ViewModel class in another PCL assembly.

It is based on the adjacent `ThreeLayerRealmXF` but uses NuGet to get the Realm libraries rather than linking to local copies. As such, it allows for quick testing of our NuGet releases in a full application context.

## The Layers

* Platform-specific app such as `ThreeLayerRealmXFNuGet.IOS` or `ThreeLayerRealmXFNuGet.Droid`.
* `ThreeLayerRealmXFNuGet` UI Layer of common Xamarin Forms UI.
* `PurePCLViewModel` Sandwich filling invoked by `ThreeLayerRealmXFNuGet`.
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

## Other Notes

`ThreeLayerRealmXFNuGet.WinPhone` is not yet completed as we lack a Realm build for WinPhone

### About the Binding

If you are unfamiliar with the binding models of WPF or Xamarin Forms this may seem convoluted to you (it is!).

if you look in the `ThreeLayerRealmXFNuGet` project at `App.cs` you can see the binding being created.

The two key things are:

1. set the `BindingContext` to something which descends from `INotityPropertyChanged` and
2. use `SetBinding` to bind to a property within that object ( `_model` in this case)

The `Model.TestRealm` method triggers a `PropertyChanged` with the important signature "TheAnswer" to indicate that property has changed and the binding should refresh our `boundLabel`.