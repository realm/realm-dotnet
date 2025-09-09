# Use Realm in a Console App - .NET SDK
## Overview
Realm instances and objects are bound to a
[SynchronizationContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext?view=net-5.0),
which means that they can only be accessed on the same thread on which they
are created. On platforms with a UI thread, the framework installs a
`SynchronizationContext` on the main thread, allowing you to make reads and
writes to the database with asynchronous calls.

However, in console apps, there is no UI thread, and thus no
`SynchronizationContext` installed. This means that if you `await` an
asynchronous Task, a random thread is spun up from the thread pool, from which
you can no longer access any previously-opened Realm instances.

To be able to efficiently use Realm between asynchronous calls, you should
install a `SynchronizationContext` - either one you implement yourself, or one
provided in a 3rd party library.
