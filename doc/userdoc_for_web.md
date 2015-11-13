---
layout: docs
tagline: "C# Docs"
---

<div class="col-md-8">
<div class="docs-wrapper">

# Realm {% binding_name %} <span class="version">{% render_version %}</span>

Realm {% binding_name %} enables you to efficiently write your app's model layer
in a safe, persisted and fast way. Here's what it looks like:

```c#
// Define your models like regular C# classes
public class Dog : RealmObject {
    public string name { get; set; }
    public int age { get; set; }
    public Owner owner { get; set; }
}

public class Person : RealmObject {
    public string Name { get; set; }
    public RealmList<Dog> Dogs { get; set; } 
}

// Use them like regular C# objects
Dog mydog = new Dog();
mydog.name = "Rex";
Debug.WriteLine(Name of dog: $"{mydog.name}");

// Persist your data easily
  Realm realm = Realm.GetInstance(Path.GetTempFileName());
  using (var trans = realm.BeginWrite()) {
      realm.addObject(mydog);
      trans.Commit();
  }

// Query it with standard LINQ, either syntax
  var r = realm.All<Dog>().Where( d => d.age > 8);
  var r2 = from d in realm.All<Dog>() where  d.age > 8 select d;
```

## Getting Started

**TBD consider if we want to break this out into multiple documents? We have up to three IDEs (inc Unity's SharpDevelop) and a range of platforms - running on OS X vs Windows**


### Installing with NuGet
**add NuGet instructions here **

### Installing from Downloaded Zip
**add download instructions here **

### Installing from Unity Store
**add download instructions here **

### Prerequisites
* Apps using Realm can target: 
	* **Apple via Xamarin:** iOS 7 or later, OS X 10.9 or later & WatchKit. 
	* **Apple via Unity:** iOS 7 or later, OS X 10.9 or later. 
	* **Android via Xamarin:** Android version ?????? or later
	* **Android via Unity:** Android version ?????? or later
	* **Windows:**  Windows Phone 8 or later, Windows Desktop 8 or later. Visual Studio 2013 or later. 

### tvOS ###
Although tvOS is in beta, we're currently evaluating what Realm would look like
on the platform. Whilst Xamarin have []early support](https://developer.xamarin.com/guides/ios/tvos/) we will not be adding C# bindings for tvOS until after the Cocoa bindings have shipped.

### Realm Browser
We also provide a [standalone Mac app named Realm Browser](https://itunes.apple.com/app/realm-browser/id1007457278) to read and edit .realm databases.

<img class="img-responsive img-rounded" src="/assets/docs/browser.png" alt="Realm Browser" />

You can generate a test database with sample data using the menu item **Tools > Generate demo database**.

If you need help finding your app’s Realm file, check this [StackOverflow answer](http://stackoverflow.com/a/28465803/3838010) for detailed instructions.

The Realm Browser is [available on the Mac App Store](https://itunes.apple.com/app/realm-browser/id1007457278).

### API Reference

**TBD link to be added after generating a C# reference**

**TBD also add some comments on how/if our code comments support Intellisense**

### Examples

**TBD prefer a sub-index of examples rather than just referring to the zip**

## Getting Help

**TBD add help instructions for now which are non-public**

## Models

Realm data models are defined using traditional C# classes with properties.
Simply subclass {{ RealmObject }} or an existing model class to create your Realm data model objects.
Realm model objects mostly function like any other C# objects - you can add your own methods and events to them and use them like you would any other object.
The main restrictions are that you can only use an object on the thread which it was created, and you use custom getters and setters for any persisted properties.

Relationships and nested data structures are modeled simply by including properties of the target type or `RealmList` for typed lists of objects.

**TBD confirm how relationships work**

````c#
public class Person;

// Dog model
public class Dog : RealmObject {
    public string name { get; set; }
    public int age { get; set; }
    public Owner owner { get; set; }
}

public class Person : RealmObject {
    public string Name { get; set; }
    public RealmList<Dog> Dogs { get; set; } 
}
````

### Controlling property persistence ###

Classes which descend from `RealmObject` are processed by the _Fody_ weaver at compilation time. All their properties that have automatic setters or getters are presumed to be persistent and have setters and getters generated to map them to the internal Realm storage. 

We also provide some C# [attributes](https://msdn.microsoft.com/en-us/library/z0w1kczw.aspx) to add metadata to control persistence.

To avoid a property being made persistent, simply add the `[Ignore]` attribute.

To have a property remapped so you can apply a custom setter, use the `[MapTo]` attribute :

```c#
        [MapTo("Email")]
        private string Email_ { get; set; }
        
        // Wrapped version of previous property
        [Ignore]
        public string Email
        {
            get { return Email_; }
            set
            {
                if (!value.Contains("@")) throw new Exception("Invalid email address");
                Email_ = value;
            }
        }
```




### Supported Types
Realm supports the following property types:  `bool`, `int`, `float`, `double`, `NSString`, `DateTime` **TBD** [truncated to the second](#nsdate-is-truncated-to-the-second), **TBD** something like `NSData`, 


You can use `RealmList<Object>` and `RealmObject` subclasses to model
relationships such as to-many and to-one.

**TBD say something about optional properties here?**


### Relationships

{{ RealmObject }}s can be linked to each other by using {{ RealmObject }} and {{ RealmList }} properties.

{{ RealmList }}s implement the standard .Net `IList` generic interface.

**TBD after finishing relationships more details on RealmList**

_Andy note - I don't like the way the Cocoa docs describe it here because the Dog class has already been described in the Model section._

<span id="to-one"></span>

#### To-One Relationships

For many-to-one or one-to-one relationships, simply declare a property with the type of your {{ RealmObject }} subclass:

```c#
public class Dog : RealmObject {
// ... other property declarations
    public Person owner;
}

public class Person : RealmObject{}
```

You can use this property like you would any other:

```c#
Person jim = new Person();
Dog rex = new Dog();
rex.owner = jim;
```


When using {{ RealmObject }} properties, you can access nested properties using normal property syntax. For example `rex.owner.address.country` will traverse the object graph and automatically fetch each object from Realm as needed.

<span id="to-many"></span>

#### To-Many Relationships

You can define a to-many relationship using {{ RealmList }} properties. {{ RealmList }}s contain other {{ RealmObject }}s of a single type and conform to `IList`.

To add a “dogs” property on our Person model that links to multiple dogs, we simply declare it as a `RealmList<Dog>` property.

````c#
public class Dog;

public class Person : RealmObject {
// ... other property declarations
    public RealmList<Dog> Dogs { get; set; } 
}
````

You can access and assign {{ RealmList }} properties as usual:

**TBD check the list syntax when completed**

````c#
// Jim is owner of Rex and all dogs named "Fido"
var someDogs = realm.All<Dog>().Where( d => d.name contains "Fido");
jim.dogs.Add(someDogs);
jim.dogs Add(rex);
````

**TBD work out equivalent of assigning nil to the RLMArray to empty it out**

#### Inverse Relationships

With inverse relationships (also known as backlinks), you can obtain all objects linking to a given object through a specific property. 

**TBD work out syntax for this that makes C# sense**

### Optional Properties

Realm stores primitives such as `int` directly, without [Boxing](https://msdn.microsoft.com/en-us/library/yz2be5wk.aspx) them as objects.

**TBD work out syntax for this that makes C# sense - do we have nulls?**

_Andy note: there's a long section for these in Cocoa with the assumption that you have to override requiredProperties to force properties to be required. The Cocoa docs (and binding?) also don't mention how nullable column work_

### Indexed Properties

**TBD work out syntax for this that makes C# sense**

Currently only strings and integers can be indexed.

Indexing a property will greatly speed up queries where the property is compared for equality (i.e. the `=` and `IN` operators), at the cost of slower insertions.

### Default Property Values

**TBD we have an outstanding issue to fix this as our weaver kills them**

### Primary Keys

**TBD work out syntax for this that makes C# sense - via attributes?**

### Ignored Properties

Use the `[Ignore]` attribute to make a property be left alone and just treated as a standard C# property.

**TBD in the pipeline we have a task to ignore if have a getter**

If you define a setter or getter function on the property then it is automatically ignored.

## Writes

<div class="alert alert-warning">
All changes to an object (addition, modification and deletion) must be done within a write transaction.
</div>

Realm objects can be instantiated and used as standalone just like regular C# objects.
To share objects between threads or re-use them between app launches you must persist them to a Realm, an operation which must be done within a write transaction.

Since write transactions incur non-negligible overhead, you should architect
your code to minimize the number of write transactions.


Because write transactions could potentially fail like any other disk IO
operations,  **TBD decide if document exceptions here**  so you can handle and recover from failures
like running out of disk space. There are no other recoverable errors. For
brevity, our code samples don't handle these errors but you certainly should in
your production applications.

### Creating Objects

When you have defined a model you can instantiate your {{ RealmObject }} subclass
and add the new instance to the Realm. Consider this simple model:



## Current Limitations

Realm is currently in beta and we are continuously adding features and fixing issues while working towards a 1.0 release. Until then, we've compiled a list of our most commonly hit limitations.

Please refer to our **TBD beta repo?** [GitHub issues](https://github.com/realm/realm-dotnet/issues) for a more comprehensive list of known issues. 


#### General Limits

Realm aims to strike a balance between flexibility and performance. In order to accomplish this goal, realistic limits are imposed on various aspects of storing information in a realm. For example:

1. Class names must be between 0 and 63 bytes in length. UTF8 characters are supported. An exception will be thrown at your app's initialization if this limit is exceeded.
2. Property names must be between 0 and 63 bytes in length. UTF8 characters are supported. An exception will be thrown at your app's initialization if this limit is exceeded.
3. **TBD binary data** properties cannot hold data exceeding 16MB in size. To store larger amounts of data, either break it up into 16MB chunks or store it directly on the file system, storing paths to these files in the realm. An exception will be thrown at runtime if your app attempts to store more than 16MB in a single property.
4. **TBD DateTime** properties may have other limitations.
5. Any single Realm file cannot be larger than the amount of memory your application would be allowed to map in iOS — this changes per device, and depends on how fragmented the memory space is at that point in time (there is a radar open about this issue: rdar://17119975). If you need to store more data, you can map it over multiple Realm files.


### Preview Limitations
Features missing from this preview version which are expected to be added prior to release:

* API reference
* DateTime fields
* Binary Data fields (e.g.: for storing pictures)
* Indexing
* specifying Primary Key
* Null values for primitive types such as int and float
* default values - the standard way of defining them is not compatible with the weaving
* More LINQ operations
* Searching by related data
* cascading deletes
* optional properties - are not currently part of C# but we will be adding a way to annotate a property, probably with an attribute, to indicate it is optional

## FAQ

#### How big is the Realm library?

Once your app is built for release, Realm should only add around **TBD measure on different platforms and confirm if we include bitcode** XXX to its size. The releases we distribute are significantly larger because iOS releases include support for the iOS and watchOS simulators, some debug symbols, and bitcode, which are all stripped by Xcode automatically when you build your app. **TBD Android releases include ???? platforms**

#### Should I use Realm in production applications?

Realm has been used in production in commercial products since 2012.

You should expect our Objective-C & Swift APIs to change as we evolve the product from community feedback — and you should expect more features & bugfixes to come along as well.

#### Do I have to pay to use Realm?

No, Realm is entirely free to use, even in commercial projects.

#### How do you guys plan on making money?

We’re actually already generating revenue selling enterprise products and services around our technology.
If you need more than what is currently in our releases or in [realm-cocoa](http://github.com/realm/realm-cocoa), we’re always happy to chat [by email](mailto:info@realm.io).
Otherwise, we are committed to developing [realm-cocoa](http://github.com/realm/realm-cocoa) in the open, and to keep it free and open-source under the Apache 2.0 license.

#### I see references to a "core" in the code, what is that?

The core is referring to our internal C++ storage engine. It is not currently open-source but we do plan on open-sourcing it also under the Apache 2.0 license once we’ve had a chance to clean it, rename it, and finalize major features inside of it. In the meantime, its binary releases are made available under the Realm Core (TightDB) Binary [License](https://github.com/realm/realm-cocoa/blob/master/LICENSE).

#### I see a network call to Mixpanel when I run my app, what is that?

Realm collects anonymous analytics when your app is run with a debugger attached, or when it runs in a simulator. This is completely anonymous and helps us improve the product by flagging which versions of Realm, iOS, OS X, or which language you target and which versions we can deprecate support for. **This call does not run when your app is in production, or running on your user’s devices** — only from inside your simulator or when a debugger is attached. You can see exactly how & what we collect, as well as the rationale for it in our [source code](https://github.com/realm/realm-cocoa/blob/master/Realm/RLMAnalytics.mm).

</div><!--/docs-wrapper -->
</div><!--/col-->

<div class="col-md-3 col-md-offset-1">
<div class="navbar-docs visible-md visible-lg" data-spy="affix" data-offset-top="180">

* toc
{:toc .nav .nav-pills .nav-stacked}

</div>
</div>
