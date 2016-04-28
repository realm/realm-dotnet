Realm is a mobile database that runs directly inside phones, tablets or wearables.

## Features

* **Mobile-first:** Realm is the first database built from the ground up to run directly inside phones, tablets and wearables.
* **Simple:** Data is directly [exposed as objects](https://realm.io/docs/xamarin/latest/#models) and [queryable by code](https://realm.io/docs/xamarin/latest/#queries), removing the need for ORM's riddled with performance & maintenance issues. Plus, we've worked hard to [keep our API down to just 4 common classes](https://realm.io/docs/xamarin/latest/api/) (RealmObject, RealmList, RealmQuery and Realm): most of our users pick it up intuitively, getting simple apps up & running in minutes.
* **Modern:** Realm supports relationships, generics, vectorization and modern C# idioms.
* **Fast:** Realm is faster than even raw SQLite on common operations, while maintaining an extremely rich feature set.

Realm Xamarin enables you to efficiently write your app's model layer in a safe, persisted and fast way. Here's what it looks like:

```csharp
// Define your models like regular C# classes
public class Dog : RealmObject 
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Person Owner { get; set; }
}

public class Person : RealmObject 
{
    public string Name { get; set; }
    public RealmList<Dog> Dogs { get; set; } 
}


// Persist your data easily
var realm = Realm.GetInstance();
realm.Write(() => 
{
    var mydog = realm.CreateObject<Dog>();
    mydog.Name = "Rex";
    mydog.Age = 9;
});

// Query it with standard LINQ, either syntax
var r = realm.All<Dog>().Where(d => d.Age > 8);
var r2 = from d in realm.All<Dog>() where  d.Age > 8 select d;
```



