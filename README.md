![Realm](https://github.com/realm/realm-dotnet/raw/master/logo.png)

Realm is a mobile database that runs directly on phones, tablets or wearables.

This repository holds the source code for the .NET / C# versions of Realm. Currently, we support all major mobile and desktop operating systems, such as iOS, Android, UWP, macOS, Linux, and Windows. For a full list of supported platforms and their versions, check out the [Prerequisites](https://realm.io/docs/dotnet/latest/#prerequisites) section in the documentation.

## Features

* **Mobile-first:** Realm is the first database built from the ground up to run directly inside phones, tablets, and wearables.
* **Simple:** Data is directly [exposed as objects](https://realm.io/docs/dotnet/latest/#models) and [queryable by code](https://realm.io/docs/dotnet/latest/#queries), removing the need for ORM's riddled with performance & maintenance issues. Plus, we've worked hard to [keep our API down to just 2 common classes](https://realm.io/docs/dotnet/latest/api/) (RealmObject and Realm): most of our users pick it up intuitively, getting simple apps up & running in minutes.
* **Modern:** Realm supports relationships, generics, vectorization and modern C# idioms.
* **Fast:** Realm is faster than even raw SQLite on common operations while maintaining an extremely rich feature set.

## Getting Started

Please see the detailed instructions in our [User Guide](https://realm.io/docs/dotnet/latest/#installation) to add Realm to your solution.

## Documentation

The documentation can be found at [realm.io/docs/dotnet/latest](https://realm.io/docs/dotnet/latest).
The API reference is located at [realm.io/docs/dotnet/latest/api](https://realm.io/docs/dotnet/latest/api).

## Getting Help

- **Need help with your code?**: Look for previous questions on the  [#realm tag](https://stackoverflow.com/questions/tagged/realm?sort=newest) — or [ask a new question](https://stackoverflow.com/questions/ask?tags=realm). We actively monitor & answer questions on SO!
- **Have a bug to report?** [Open an issue](https://github.com/realm/realm-dotnet/issues/new). If possible, include the version of Realm, a full log, the Realm file, and a project that shows the issue.
- **Have a feature request?** [Open an issue](https://github.com/realm/realm-dotnet/issues/new). Tell us what the feature should do, and why you want the feature.
- Sign up for our [**Community Newsletter**](https://realm.io/realm-news-subscribe) to get regular tips, learn about other use-cases and get alerted to blog posts and tutorials about Realm.

## Nightly builds

If you want to test recent bugfixes or features that have not been packaged in an official release yet, you can use the preview releases published after every
commit to the [realm-nightly](https://www.myget.org/feed/Packages/realm-nightly) MyGet feed. Refer to [this guide](https://www.visualstudio.com/en-us/docs/package/nuget/consume)
for instructions on adding custom sources to the NuGet Package Manager. The source URL you need to specify is `https://www.myget.org/F/realm-nightly/api/v3/index.json`.

## Building Realm

We highly recommend [using our pre-built binaries via NuGet](https://realm.io/docs/dotnet/latest/#installation) but you can also build from source.

Prerequisites:

* Visual Studio Community or above.
* Building Xamarin iOS apps also requires Xcode 8.1.

We support the current Xamarin _Stable_ update channel, at the time of release this corresponded to:

* Xamarin iOS version 10.3.1.8
* Xamarin Android version 7.0.2.42
* Xamarin Studio version 6.1.4

**Note for Debugging** that the following steps mention building for **Release.** If you are debugging, just substitute **Debug** and you probably also want to choose **Debug | iPhoneSimulator** as a platform.

1. Download and build the native libraries using the instructions in `wrappers/README.md`
1. Open the `Realm.sln` in `Visual Studio`
1. Build `RealmWeaver.Fody` and `Realm.BuildTasks`
1. Build `Realm` and `Realm.Sync`
1. Build and run the tests for the relevant platforms.

If you are actively testing code against the Realm source, see also the unit test projects and other tests under the Tests folder.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details!

This project adheres to the [Contributor Covenant Code of Conduct](https://realm.io/conduct).
By participating, you are expected to uphold this code. Please report
unacceptable behavior to [info@realm.io](mailto:info@realm.io).

## License

Realm .NET is published under the Apache 2.0 license.
Realm Core is also published under the Apache 2.0 license and is available
[here](https://github.com/realm/realm-core).

**This product is not being made available to any person located in Cuba, Iran,
North Korea, Sudan, Syria or the Crimea region, or to any other person that is
not eligible to receive the product under U.S. law.**

## Feedback

**_If you use Realm and are happy with it, all we ask is that you please consider sending out a tweet mentioning [@realm](https://twitter.com/realm) to share your thoughts!_**

**_And if you don't like it, please let us know what you would like improved, so we can fix it!_**

![analytics](https://ga-beacon.appspot.com/UA-50247013-2/realm-dotnet/README?pixel)
