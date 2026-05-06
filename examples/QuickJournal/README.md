# QuickJournal

**QuickJournal** is a simple MAUI application that shows how Realm can be used effectively in conjunction with MVVM and data binding.
The app allows the user to keep a very minimal journal, where each entry is made up of a title and a body. Every time a new journal entry is added or modified it gets persisted to a realm, and thanks to the bindings the UI gets updated immediately, with no additional code required.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- .NET MAUI workload: `dotnet workload install maui`
- Platform-specific tooling (Xcode for iOS/macOS, Android SDK for Android, Windows App SDK for Windows)

## Supported Platforms

- Android (API 21+)
- iOS 13.0+
- macOS 12.0+ (via Mac Catalyst)
- Windows 10 1809+ (WinUI 3)