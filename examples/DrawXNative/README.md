# Realm Draw Demo

Realm Draw is a simple drawing app designed to show off the collaborative features of the [Realm Mobile Platform](https://realm.io/news/introducing-realm-mobile-platform/).

Any number of users may draw on a single shared canvas in any given moment, with contributions from other devices appearing on the canvas in real-time.

This version is a write from scratch using Xamarin and the [SkiaSharp](https://github.com/mono/SkiaSharp) drawing framework, to interoperate with the iOS version written in Objective-C. It uses native UI projects with common PCL logic.

To allow testers to have both this and the original on the same device, it is called DrawX.

## Installation Instructions

1. [Download the macOS version](https://realm.io/docs/realm-mobile-platform/get-started/) of the Realm Mobile Platform.
2. Run a local instance of the Realm Mobile Platform.
3. Create a user with the email 'demo@realm.io' and the password 'demo'.
4. Build the Draw app and deploy it to iOS or Android devices on the same network as your Mac.