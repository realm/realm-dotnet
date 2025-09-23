# Delete Data - .NET SDK
## Delete an Object
> **EXAMPLE:**
> The following code shows how to delete one object from
its realm:
>
> ```csharp
> realm.Write(() =>
> {
>     // Remove the instance from the realm.
>     realm.Remove(dog);
>
>     // Discard the reference.
>     dog = null;
> });
>
> ```
>

## Delete Multiple Objects
> **EXAMPLE:**
> The following code demonstrates how to delete a
collection from a realm:
>
> ```csharp
> realm.Write(() =>
> {
>     // Find dogs younger than 2 years old.
>     var puppies = realm.All<Dog>().Where(dog => dog.Age < 2);
>
>     // Remove the collection from the realm.
>     realm.RemoveRange(puppies);
> });
>
> ```
>

## Delete an Object and its Dependent Objects
Sometimes, you have dependent objects that you want to delete when
you delete the parent object. We call this a **chaining delete**.
Realm will not delete the dependent objects for
you. If you do not delete the objects yourself, they will
remain orphaned in your realm. Whether or not this is a
problem depends on your application's needs.

Currently, the best way to delete dependent objects is to
iterate through the dependencies and delete them before
deleting the parent object.

> **EXAMPLE:**
> The following code demonstrates how to perform a
chaining delete by first deleting all of Ali's dogs,
then deleting Ali:
>
> ```csharp
> realm.Write(() =>
> {
>     // Remove all of Ali's dogs.
>     realm.RemoveRange(ali.Dogs);
>
>     // Remove Ali.
>     realm.Remove(ali);
> });
>
> ```
>

## Delete All Object of a Specific Type
Realm supports deleting all instances of a
Realm type from a
realm.

> **EXAMPLE:**
> The following code demonstrates how to delete all
Dog instances from a realm:
>
> ```csharp
> realm.Write(() =>
> {
>     // Remove all instances of Dog from the realm.
>     realm.RemoveAll<Dog>();
> });
>
> ```
>

## Delete All Objects in a Realm
It is possible to delete all objects from the realm. This
does not affect the schema of the realm. This is useful for
quickly clearing out your realm while prototyping.

> **EXAMPLE:**
> The following code demonstrates how to delete everything
from a realm:
>
> ```csharp
> realm.Write(() =>
> {
>     // Remove all objects from the realm.
>     realm.RemoveAll();
> });
>
> ```
>
