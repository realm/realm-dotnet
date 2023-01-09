# QuickJournal

**QuickJournal** is a simple MAUI application that shows how Realm can be used effectively in conjunction with MVVM and data binding.
The app allows the user to keep a very minimal journal, where each entry is made up of a title and a body. Every time a new journal entry is added or modified it gets persisted to a realm, and thanks to the bindings the UI gets updated immediately, with no additional code required. 