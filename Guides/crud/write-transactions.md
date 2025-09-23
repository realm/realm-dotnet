# Write Transactions - .NET SDK
## Overview
In addition to reading objects, you can create, update, and delete
objects from a realm. Because these operations modify the
state of the realm, we call them "writes", and **all writes to Realm
must be within a transaction**.

A transaction is a list of read and write operations that
Realm treats as a single indivisible operation. A transaction is an
*all or nothing* operation: either all of the operations in the transaction
succeed or none of the operations in the transaction take effect.

A realm allows only one open transaction at a time. Realm
blocks other writes on other threads until the open
transaction is complete. This ensures that there is no race
condition when reading values from the realm within a
transaction.

When all operations in a transaction complete, Realm either
commits it or cancels the transaction:

- When Realm commits a transaction, Realm writes
all changes to disk.
- When Realm cancels a write transaction (as when an operation in
the transaction causes an error), all changes are discarded
(or "rolled back").

## Initiate a Transaction
The .NET SDK provides a simple API you can use for most writes.  The
`Write()` and
`WriteAsync()` and
methods wrap all commands into a single transaction and then commit the
transaction.

The `Write()` and `WriteAsync()` methods are shorthand for using the
`BeginWrite()` and
`Transaction.Commit()`
methods, and their `Async` counterparts. In most cases, either of these two write
methods will meet your needs. If you need finer control over the transaction,
use `BeginWrite()` with one of these `Transaction` methods:

- `Commit()`
- `Dispose()`
- `Rollback()`

The following code shows how to use both approaches:

```csharp
// Instantiate a class, as normal.
var dog = new Dog { Id = 42, Name = "Max", Age = 5 };
// Open a thread-safe transaction.
realm.Write(() =>
{
    // Add the instance to the realm.
    realm.Add(dog);
});

```

```csharp
// Open a thread-safe transaction.
using (var transaction = realm.BeginWrite())
{
    // At this point, the TransactionState is "Running":
    // transaction.State == TransactionState.Running
    try
    {
        // Perform a write op...
        realm.Add(myDog);
        // Do other work that needs to be included in
        // this transaction
        if (transaction.State == TransactionState.Running)
        {
            transaction.Commit();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        // Something went wrong; roll back the transaction
        if (transaction.State != TransactionState.RolledBack &&
            transaction.State != TransactionState.Committed)
        {
            transaction.Rollback();
        }
    }
}

```

> **NOTE:**
> As with most C# async methods, the
`WriteAsync`,
`BeginWriteAsync`,
and `CommitAsync`
methods can take a
[CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken?view=net-6.0)
as a parameter, providing you with the option to cancel the operation before
the process completes.
>

## Check the Status of a Transaction
The SDK provides a `TransactionState`
property with the current transaction status. You can use `TransactionState`
to ensure you don't commit or rollback a transaction twice:

```csharp
// Open a thread-safe transaction.
using (var transaction = realm.BeginWrite())
{
    // At this point, the TransactionState is "Running":
    // transaction.State == TransactionState.Running
    try
    {
        // Perform a write op...
        realm.Add(myDog);
        // Do other work that needs to be included in
        // this transaction
        if (transaction.State == TransactionState.Running)
        {
            transaction.Commit();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        // Something went wrong; roll back the transaction
        if (transaction.State != TransactionState.RolledBack &&
            transaction.State != TransactionState.Committed)
        {
            transaction.Rollback();
        }
    }
}

```
