# Manually Define a Schema - .NET SDK
When Realm processes Realm objects, it
generates a schema for each class based on the class properties.
However, there may be times that you want to manually define the schema,
and the .NET SDK provides a mechanism for doing so.

> **IMPORTANT:**
> All Realm objects inherit from the
`IRealmObject`,
`IEmbeddedObject`, or
`IAsymmetricObject`
interface and must be declared `partial` classes.
>
> In versions of the .NET SDK older than 10.18.0, objects derive from
`RealmObject`,
`EmbeddedObject`, or
`AsymmetricObject`
base classes. This approach to Realm model definition is still supported, but
does not include new features such as the nullability annotations. In a future SDK release, the
base classes will become deprecated. You should use the interfaces for any
new classes that you write and should consider migrating your existing
classes.
>

## Use the Schema Property
You use the
`Schema`
property of the
`RealmConfigurationBase`
object to control how schemas are defined. The following code example shows three
ways to do this, from easiest to most complex: automatic configuration, manual
configuration, and a mix of both methods.

```csharp
// By default, all loaded RealmObject classes are included.
// Use the RealmConfiguration when you want to
// construct a schema for only specific C# classes:
var config = new RealmConfiguration
{
    Schema = new[] { typeof(ClassA), typeof(ClassB) }
};

// More advanced: construct the schema manually
var manualConfig = new RealmConfiguration
{
    Schema = new RealmSchema.Builder
    {
        new Builder("ClassA", ObjectType.EmbeddedObject)
        {
            Property.Primitive("Id",
                RealmValueType.Guid,
                isPrimaryKey: true),
            Property.Primitive("LastName",
                RealmValueType.String,
                isNullable: true,
                indexType: IndexType.General)
        }
    }
};

// Most advanced: mix and match
var mixedSchema = new ObjectSchema.Builder(typeof(ClassA));
mixedSchema.Add(Property.FromType<int>("ThisIsNotInTheCSharpClass"));
// `mixedSchema` now has all of the properties of the ClassA class
// and an extra integer property called "ThisIsNotInTheCSharpClass"

var mixedConfig = new RealmConfiguration
{
    Schema = new[] { mixedSchema.Build() }
};

```
