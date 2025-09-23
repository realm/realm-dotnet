# Threading - .NET SDK
To make your C#/.NET apps fast and responsive,
you must balance the computing time needed to lay out the
visuals and handle user interactions with the time needed to
process your data and run your business logic. Typically,
app developers spread this work across multiple threads: the
main or UI thread for all of the user interface-related
work, and one or more background threads to compute heavier
workloads before sending it to the UI thread for
presentation. By offloading heavy work to background
threads, the UI thread can remain highly responsive
regardless of the size of the workload. But it can be
notoriously difficult to write thread-safe, performant, and
maintainable multithreaded code that avoids issues like
deadlocking and race conditions. Realm aims to
simplify this for you.

> **IMPORTANT:**
> Throughout this page, we refer to the "main thread" (or "UI thread") and
"background threads". To be more accurate, any mention of the main or UI thread
refers to any thread with a
[SynchronizationContext](https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext),
while those threads without a `SynchronizationContext` are considered
background threads.
>

## Three Rules to Follow
Before exploring Realm's tools for multithreaded apps, you need to
understand and follow these three rules:

Realm's Multiversion Concurrency Control (MVCC)
architecture eliminates the need to lock for read operations. The values you
read will never be corrupted or in a partially-modified state. You can freely
read from the same Realm file on any thread without the need for locks or
mutexes. Unnecessarily locking would be a performance bottleneck since each
thread might need to wait its turn before reading.

You can write to a Realm file from any
thread, but there can be only one writer at a time. Synchronous
write transactions block each other. So, a synchronous write on the main thread
may result in your app appearing unresponsive while it waits for a write on a
background thread to complete. To prevent this, the SDK provides the
`WriteAsync()`
method. For more information, see Asynchronous Writes.

Live objects, collections, and realm instances are
**thread-confined**: that is, they are only valid on the
thread on which they were created. Practically speaking,
this means you cannot pass live instances to other
threads. However, Realm offers several mechanisms for
sharing objects across threads.

## Communication Across Threads
To access the same Realm file from different threads, you must instantiate a
realm instance on every thread that needs access. As long as you specify the same
configuration, all realm instances will map to the same file on disk.

One of the key rules when working with Realm in a multithreaded
environment is that objects are thread-confined: **you cannot access the
instances of a realm, collection, or object that originated on other threads.**
Realm's Multiversion Concurrency Control (MVCC)
architecture means that there could be many active versions of an object at any
time. Thread-confinement ensures that all instances in that thread are of the
same internal version.

When you need to communicate across threads, you have
several options depending on your use case:

- To modify an object on two threads, query
for the object on both threads.
- To react to changes made on any thread, use Realm's
notifications.
- To see changes that happened on another thread in the current thread's realm
instance, refresh your realm instance.
- To send a fast, read-only view of the object to other threads,
"freeze" the object.
- To keep and share many read-only views of the object in your app, copy
the object from the realm.

## Refreshing Realms
On the main UI thread (or any thread with a run loop), Realm
automatically refreshes objects at the start of every run loop iteration.
Between run loop iterations, you will be working on the snapshot, so
individual methods always see a consistent view and never have to worry
about what happens on other threads.

When you initially open a realm on a thread, its state will be the most
recent successful write commit, and it will remain on that version until
refreshed. If a thread has no run loop (which is generally the case in a
background thread), then the
`Realm.Refresh()`
method must be called manually in order to advance the transaction to the most
recent state.

Realms are also refreshed when write transactions are committed with
`Transaction.Commit()`.

> **NOTE:**
> Failing to refresh Realms on a regular basis could lead to some transaction
versions becoming "pinned", preventing Realm from reusing the disk space used
by that version, leading to larger file sizes.
>

## Asynchronous Writes
The `WriteAsync()`
method provides a simple way to offload the UI thread. Realm will asynchronously
begin and commit the transaction, but the actual write block will execute
on the original thread. So, waiting for changes is asynchronous, but the callback
is executed on the main thread. This means that objects and queries created
before the write block can be used inside the block without relying on
threadsafe references.

The following code shows two examples of creating an object with `AsyncWrite()`.

```csharp
var testItem = new Item
{
    Name = "Do this thing",
    Status = ItemStatus.Open.ToString(),
    Assignee = "Aimee"
};

await realm.WriteAsync(() =>
{
    realm.Add(testItem);
});

// Or

var testItem2 =
    await realm.WriteAsync(() =>
    {
        return realm.Add<Item>(new Item
        {
            Name = "Do this thing, too",
            Status = ItemStatus.InProgress.ToString(),
            Assignee = "Satya"
        });
    }
);

```

> **NOTE:**
> If you call `WriteAsync()` in a background thread, Realm runs synchronously on
the thread, so it is the equivalent of calling
`Write()`.
>

## Frozen Objects
Live, thread-confined objects work fine in most cases.
However, some apps -- those based on reactive, event
stream-based architectures, for example -- need to send
immutable copies around to many threads for processing
before ultimately ending up on the UI thread. Making a deep
copy every time would be expensive, and Realm does not allow
live instances to be shared across threads. In this case,
you can **freeze** objects, collections, and realms.

Freezing creates an immutable view of a specific object,
collection, or realm that still exists on disk and does not
need to be deeply copied when passed around to other
threads. You can freely share a frozen object across threads
without concern for thread issues.

When working with frozen objects, an attempt to do any of
the following throws an exception:

- Opening a write transaction on a frozen realm.
- Modifying a frozen object.
- Adding a change listener to a frozen realm, collection, or object.

Once frozen, it is not possible to unfreeze an object. You
can use the `IsFrozen` method to check if the object is frozen.
This method is always thread-safe.

To modify a frozen object, query for it on an unfrozen
realm, then modify it.

Frozen objects are not live and do not automatically update.
They are effectively snapshots of the object state at the
time of freezing.

When you freeze a realm, its child objects also become
frozen.

Frozen objects remain valid as long as the live realm that
spawned them stays open. Therefore, avoid closing the live
realm until all threads are done with the frozen objects.
You can close frozen realm before the live realm is closed.

> **IMPORTANT:**
> Caching too many frozen objects can have a negative
impact on the realm file size. "Too many" depends on your
specific target device and the size of your Realm
objects. If you need to cache a large number of versions,
consider copying what you need out of the realm instead.
>

## Realm's Threading Model in Depth
Realm provides safe, fast, lock-free, and concurrent access
across threads with its [Multiversion Concurrency
Control (MVCC)](https://en.wikipedia.org/wiki/Multiversion_concurrency_control)
architecture.

### Compared and Contrasted with Git
If you are familiar with a distributed version control
system like [Git](https://git-scm.com/), you may already
have an intuitive understanding of MVCC. Two fundamental
elements of Git are:

- Commits, which are atomic writes.
- Branches, which are different versions of the commit history.

Similarly, Realm has atomically-committed writes in the form
of transactions. Realm also has many
different versions of the history at any given time, like
branches.

Unlike Git, which actively supports distribution and
divergence through forking, a realm only has one true latest
version at any given time and always writes to the head of
that latest version. Realm cannot write to a previous
version. This makes sense: your data should converge on one
latest version of the truth.

### Internal Structure
A realm is implemented using a [B+ tree](https://en.wikipedia.org/wiki/B%2B_tree) data structure. The top-level node represents a
version of the realm; child nodes are objects in that
version of the realm. The realm has a pointer to its latest
version, much like how Git has a pointer to its HEAD commit.

Realm uses a copy-on-write technique to ensure
[isolation](https://en.wikipedia.org/wiki/Isolation_(database_systems)) and
[durability](https://en.wikipedia.org/wiki/Durability_(database_systems)).
When you make changes, Realm copies the relevant part of the
tree for writing. Realm then commits the changes in two
phases:

- Realm writes changes to disk and verifies success.
- Realm then sets its latest version pointer to point to the newly-written version.

This two-step commit process guarantees that even if the
write failed partway, the original version is not corrupted
in any way because the changes were made to a copy of the
relevant part of the tree. Likewise, the realm's root
pointer will point to the original version until the new
version is guaranteed to be valid.

> **EXAMPLE:**
> The following diagram illustrates the commit process:
>
> ![Realm copies the relevant part of the tree for writes, then replaces the latest version by updating a pointer.](/images/mvcc-diagram.png)
>
> 1. The realm is structured as a tree.  The realm has a pointer
to its latest version, V1.
> 2. When writing, Realm creates a new version V2 based on V1.
Realm makes copies of objects for modification (A 1,
C 1),  while links to unmodified objects continue to
point to the original versions (B, D).
> 3. After validating the commit, Realm updates the
pointer to the new latest version, V2. Realm then discards
old nodes no longer connected to the tree.
>

Realm uses zero-copy techniques
like memory mapping to handle data. When you read a value
from the realm, you are virtually looking at the value on
the actual disk, not a copy of it. This is the basis for
live objects. This is also why a realm
head pointer can be set to point to the new version after
the write to disk has been validated.

## Summary
- Realm enables simple and safe multithreaded code when you follow
three rules: don't lock to readavoid synchronous writes on the UI thread if you write on background threads.
- There is a proper way to share objects across threads for each use case.
- In order to see changes made on other threads in your realm
instance, you must manually **refresh** realm instances that do
not exist on "loop" threads or that have auto-refresh disabled.
- For apps based on reactive, event-stream-based architectures, you can
**freeze** objects, collections, and realms in order to pass
shallow copies around efficiently to different threads for processing.
- Realm's multiversion concurrency control (MVCC)
architecture is similar to Git's. Unlike Git, Realm has
only one true latest version for each realm.
- Realm commits in two stages to guarantee isolation and durability.
