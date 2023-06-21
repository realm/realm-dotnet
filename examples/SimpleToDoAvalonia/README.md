# simple-to-do-avalonia

**SimpleToDo** is a simple desktop [Avalonia](https://github.com/AvaloniaUI/Avalonia) application that shows how Realm
can be used effectively in conjunction with MVVM and data binding.

The app allows to user to keep a collection of several to-do lists, where each to-do list is comprised of different
items, that have a description and can be marked as done.
Thanks to the automatic write transactions and bindings, all the changes done by the users in the app are automatically
persisted into a realm. At the same time, all the changes to realm objects and collections done in code are directly
reflected in the UI, with no additional code required. 

