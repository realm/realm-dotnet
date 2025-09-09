# Data Binding - .NET SDK
In .NET applications built with Xamarin, MAUI, Avalonia UI and other MVVM-based
frameworks, data binding enables UI updates when an underlying object or
collection changes and vice versa.

Realm objects and collections are live and update
automatically to reflect data changes. This is because
Realm objects implement
[INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged)
and Realm collections implement
[INotifyCollectionChanged](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged).
When you bind live collections and objects to your UI, both the UI and live
objects and collections update at the same time.

> **NOTE:**
> The Realm SDK does not support [Compiled Bindings](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/data-binding/compiled-bindings).
>

## Data Binding Examples
The following code snippets show both the C# and XAML for three typical
data-binding use cases. They all use the following Realm objects:

```csharp
public partial class Employee : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("employee_id")]
    public string EmployeeId { get; set; }

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("items")]
    public IList<Item> Items { get; }
}

public partial class Item : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("owner_id")]
    public string OwnerId { get; set; }

    [MapTo("summary")]
    public string Summary { get; set; }

    [MapTo("isComplete")]
    public bool IsComplete { get; set; }
}

```

> **NOTE:**
> Because Realm collections are live, you should not call `ToList()` when
obtaining a collection of Realm objects. Doing so forces loading the
collection from memory, so the resulting collection is no longer a live
object.
>

### Binding to a Single Realm Object
In the following example, we create a public property of type `Employee`.
Elsewhere in our class, we query our realm for the `Employee` with an
"EmployeeId" of "123":

```csharp
public Employee Employee123 { get; }
...
Employee123 = realm.All<Employee>()
    .FirstOrDefault(e => e.EmployeeId == "123");

```

In our XAML, we bind the Employee's `Name` property to a `Label` control's
`Text` property:

```xml
<Label Text="{Binding Employee123.Name}"/>

```

### Binding to a Realm Collection
There are two types of collections we can bind to. First, we can bind to a
collection of Realm objects. In this code example, we create a public
`IEnumerable` of the `Employee` Realm object and then populate that
collection with all of the objects in the Employees collection:

```csharp
public IEnumerable<Employee> Employees { get; }
...
Employees = realm.All<Employee>();

```

In the associated XAML file, we bind a `ListView` to the Employees
`IEnumerable` to show a list of all Employees. In this case, we're listing
each employee's "EmployeeId" value:

```xml
<ListView x:Name="listAllEmployees" ItemsSource="{Binding Employees}">
   <ListView.ItemTemplate>
        <DataTemplate>
            <ViewCell>
                <Label Text="{Binding EmployeeId}"/>
            </ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>

```

We can also bind to a *collection property* of a Realm object. In this example,
we bind to the `Items` property of a specific `Employee`. The `Items`
property on the `Employee` class is of type `IList<Item>`.

```csharp
public Employee Employee123 { get; }
...
IncompleteItems = Employee123.Items
    .AsRealmQueryable()
    .Where(i => i.IsComplete == false);

```

> **NOTE:**
> Note that we are calling the `AsRealmQueryable()`
method. This is required because we are filtering the collection. If we want
to list all of the Items in the `Items` collection, we do not need to call
`AsRealmQueryable()`.
>

The XAML implementation is similar to the example above:

```xml
<ListView x:Name="listIncompleteItems" ItemsSource="{Binding IncompleteItems}">
   <ListView.ItemTemplate>
        <DataTemplate>
            <ViewCell>
                <Label Text="{Binding Summary}"/>
            </ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>

```

## Data Binding and MVVM
When building a .NET app using the
[MVVM](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/xaml/xaml-basics/data-bindings-to-mvvm)
pattern, you often want proper data binding *in both directions* between the
View and ViewModel: if a property value changes in the UI, the ViewModel should
be notified, and vice-versa. ViewModels typically implement
the [INotifyPropertyChange](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0)
interface to ensure this.

However, when you bind UI elements to a `RealmObject`,
this notification happens automatically, thus greatly simplifying your ViewModel code.

You can also bind a UI element to a realm Live Query.
For example, if you bind a
[ListView](https://docs.microsoft.com/en-us/dotnet/api/xamarin.forms.listview?view=xamarin-forms)
to a live query, then the list will update automatically when the results of the
query change; you do not need to implement the `INotifyPropertyChange`
interface.

For example, if you bind a ListView to a query that returns all Product
objects, the list of available products updates automatically
when a product is added, changed, or removed from the realm:

```csharp
// Somewhere in your ViewModel
AllProducts = realm.All<Products>();
```

```xaml
<!-- Somewhere in your View -->
<ListView ItemsSource="{Binding AllProducts}">
```
