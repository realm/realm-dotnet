# Realm Draw Demo

Realm Draw is a simple drawing app designed to show off the collaborative features of the [Realm Mobile Platform](https://realm.io/news/introducing-realm-mobile-platform/).

Any number of users may draw on a single shared canvas in any given moment, with contributions from other devices appearing on the canvas in real-time.

This version is a write from scratch using Xamarin and the [SkiaSharp](https://github.com/mono/SkiaSharp) drawing framework, to interoperate with the iOS version written in Objective-C. It uses native UI projects with common PCL logic.

To allow testers to have both this and the original on the same iOS device, it is called DrawX.

## Local Versions of projects

Note for our testing purposes, the `DrawXNativeLocal` variants use an adjacent Realm project directly rather than the Realm from NuGet, to be able to pull in sync code in progress.

**Note:** due to the way that the IOS designer.cs files are generated, instead of a shared link to `ViewController.cs`, an immediate adjacent copy is required.

## Installation Instructions

1. [Download the macOS version](https://realm.io/docs/realm-mobile-platform/get-started/) of the Realm Mobile Platform.
2. Run a local instance of the Realm Mobile Platform.
3. Create any users you wish to use from the apps, in the web dashboard that appears when you launch the server. They do not need to be admin users.
4. Build the Draw app and deploy it to iOS or Android devices able to reach your Mac, either on the same network or from other locations provided your firewall allows ports 9080 and 27800.
5. Login from those devices using the URL of your server from 2. and the usernames and passwords you created.