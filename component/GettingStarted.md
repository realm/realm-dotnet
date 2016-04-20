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

Look into the documentation on [realm.io/docs/xamarin/latest/](https://realm.io/docs/xamarin/latest/).

