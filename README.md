![Realm](https://github.com/realm/realm-dotnet/raw/master/logo.png)

Realm is a mobile database that runs directly on phones, tablets or wearables.

This repository holds the source code for the .NET / C# versions of Realm. Currently, we support all major mobile and desktop operating systems, such as iOS, Android, UWP, macOS, Linux, and Windows. For a full list of supported platforms and their versions, check out the [Supported Platforms](https://docs.mongodb.com/realm/dotnet/#supported-platforms) sub-section in the documentation.

## Features

* **Mobile-first:** Realm is the first database built from the ground up to run directly inside phones, tablets, and wearables.
* **Simple:** Data is directly [exposed as objects](https://docs.mongodb.com/realm/dotnet/objects/) and [queryable by code](https://docs.mongodb.com/realm/dotnet/query-engine/), removing the need for ORM's riddled with performance & maintenance issues. Plus, we've worked hard to [keep our API down to just a few common classes](https://docs.mongodb.com/realm-sdks/dotnet/latest/): most of our users pick it up intuitively, getting simple apps up & running in minutes.
* **Modern:** Realm supports relationships, generics, vectorization and modern C# idioms.
* **Fast:** Realm is faster than even raw SQLite on common operations while maintaining an extremely rich feature set.

## Getting Started

Please see the detailed instructions in our [User Guide](https://docs.mongodb.com/realm/dotnet/install/) to add Realm to your solution.

## Documentation

The documentation can be found at [docs.mongodb.com/realm/dotnet/](https://docs.mongodb.com/realm/dotnet/).
The API reference is located at [docs.mongodb.com/realm-sdks/dotnet/latest/](https://docs.mongodb.com/realm-sdks/dotnet/latest/).

## Getting Help

- **Need help with your code?**: Look for previous questions on the  [#realm tag](https://stackoverflow.com/questions/tagged/realm?sort=newest) — or [ask a new question](https://stackoverflow.com/questions/ask?tags=realm). You can also check out our [Community Forum](https://developer.mongodb.com/community/forums/tags/c/realm/9/realm-sdk) where general questions about how to do something can be discussed.
- **Have a bug to report?** [Open an issue](https://github.com/realm/realm-dotnet/issues/new). If possible, include the version of Realm, a full log, the Realm file, and a project that shows the issue.
- **Have a feature request?** [Open an issue](https://github.com/realm/realm-dotnet/issues/new). Tell us what the feature should do, and why you want the feature.

## Nightly builds

If you want to test recent bugfixes or features that have not been packaged in an official release yet, you can use the preview releases published after every
commit to the [GitHub packages](https://github.com/realm/realm-dotnet/packages) NuGet feed. To consume the packages, you have two options:
1. Refer to [this guide](https://www.visualstudio.com/en-us/docs/package/nuget/consume) for instructions on adding custom sources to the NuGet Package Manager.
    - The source URL you need to specify is `https://nuget.pkg.github.com/realm/index.json`. 
    - For `username`, specify your GitHub username
    - For password, specify an access token with the `read:packages` scope.
  
    There is an [ongoing thread](https://github.community/t/download-from-github-package-registry-without-authentication/) with the GitHub team to 
    streamline this by not requiring authentication for public packages, but as of Sept. 2020, this is still needed.
1. Download the package locally and [install it](https://stackoverflow.com/questions/10240029/how-do-i-install-a-nuget-package-nupkg-file-locally).

## Building Realm

We highly recommend [using our pre-built binaries via NuGet](https://docs.mongodb.com/realm/dotnet/install/#open-the-nuget-package-manager) but you can also build from [source](https://github.com/realm/realm-dotnet).

Prerequisites:

* Visual Studio 2019 Community or above.
* Building iOS/macOS apps also requires Xcode 8.1 or above.

**Note for Debugging** that the following steps mention building for **Release.** If you are debugging, just substitute **Debug** and you probably also want to choose **Debug | iPhoneSimulator** as a platform.

1. Download and build the native libraries using the instructions in [`wrappers/README.md`](wrappers/README.md)
1. Open the `Realm.sln` in `Visual Studio`
1. Build `Realm.Fody` and `Realm`
1. Build and run the tests for the relevant platforms.

If you are actively testing code against the Realm source, see also the unit test projects and other tests under the Tests folder.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details!

This project adheres to the [Contributor Covenant Code of Conduct](https://www.mongodb.com/community-code-of-conduct).
By participating, you are expected to uphold this code. Please report
unacceptable behavior to [info@realm.io](mailto:info@realm.io).

## License

Realm .NET and [Realm Core](https://github.com/realm/realm-core) are published under the Apache License 2.0.

**This product is not being made available to any person located in Cuba, Iran,
North Korea, Sudan, Syria or the Crimea region, or to any other person that is
not eligible to receive the product under U.S. law.**

## Feedback

**_If you use Realm and are happy with it, all we ask is that you please consider sending out a tweet mentioning [@realm](https://twitter.com/realm) to share your thoughts!_**

**_And if you don't like it, please let us know what you would like improved, so we can fix it!_**
