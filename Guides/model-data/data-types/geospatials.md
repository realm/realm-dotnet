# Geospatial Data Types - .NET SDK
> Version added: 11.1.0

Geospatial data, or "geodata", specifies points and geometric objects on the Earth's
surface. With the geodata types, you can create queries that check whether a given
point is contained within a shape. For example, you can find all coffee shops within
15 km of a specified point.

## Geospatial Data Types
The SDK supports geospatial queries using the following data types:

- GeoPoint
- GeoCircle
- GeoBox
- GeoPolygon

The SDK provides these geospatial data types to simplify querying geospatial data. You *cannot* persist these data types directly.

For information on how to persist geospatial data, refer to the
Persisting GeoPoint Data section on this page.

### GeoPoint
A `GeoPoint` defines a specific
location on the Earth's surface. All of the geospatial data types use `GeoPoints`
to define their location.

> **NOTE:**
> In methods that take a `GeoPoint`, you can alternatively provide a tuple of
doubles, where the first double is the latitude and the second double is the
longitude. The SDK interprets these tuples as `GeoPoints`. The examples on
this page demonstrate both approaches.
>

### GeoCircle
A `GeoCircle` defines a circle on
the Earth's surface. You define a `GeoCircle` by providing a `GeoPoint`
for the center of the circle and a `Distance`
object to specify the radius of the circle.

> **NOTE:**
> You can define the radius in kilometers, miles, degrees, or radians.
>

The following code shows two examples of creating a circle:

```csharp
var circle1 = new GeoCircle((47.8, -122.6),
    Distance.FromKilometers(44.4));
var circle2 = new GeoCircle(
    new GeoPoint(latitude: 47.3, longitude: -121.9),
    Distance.FromDegrees(0.25));

```

![Two GeoCircles](/images/geocircles.png)

### GeoBox
A `GeoBox` defines a rectangle on
the Earth's surface. You define the rectangle by specifying the bottom left
(southwest) corner and the top right (northeast) corner. The following example
creates 2 boxes:

```csharp
var box1 = new GeoBox(bottomLeftCorner: (47.3, -122.7),
    topRightCorner: (48.1, -122.1));

var box2 = new GeoBox(new GeoPoint(47.5, -122.4),
   new GeoPoint(47.9, -121.8));

```

![2 GeoBoxes](/images/geoboxes.png)

### GeoPolygon
A `GeoPolygon` defines a polygon
on the Earth's surface. Because a polygon is a closed shape, you must provide a
minimum of 4 points: 3 points to define the polygon's shape, and a fourth to
close the shape.

> **IMPORTANT:**
> The fourth point in a polygon must be the same as the first point.
>

You can also exclude areas within a polygon by defining one or more "holes". A
hole is another polygon whose bounds fit completely within the outer polygon.
The following example creates 3 polygons: one is a basic polygon with 5 points,
one is the same polygon with a single hole, and the third is the same polygon
with two holes:

```csharp
var basicPolygon = new GeoPolygon((48, -122.8),
    (48.2, -121.8), (47.6, -121.6), (47.0, -122.0),
    (47.2, -122.6), (48, -122.8));

// Create a polygon with a single hole
var outerRing = new GeoPoint[] {
    (48, -122.8), (48.2, -121.8),
    (47.6, -121.6), (47.0, -122.0), (47.2, -122.6),
    (48, -122.8) };

var hole1 = new GeoPoint[] {
    (47.8, -122.6), (47.7, -122.2),
    (47.4, -122.6), (47.6, -122.5),
    (47.8, -122.6) };

var polygonWithOneHole = new GeoPolygon(outerRing, hole1);

// Add a second hole to the polygon
var hole2 = new GeoPoint[] {
    (47.55, -122.05), (47.5, -121.9),(47.3, -122.1),
    (47.55, -122.05) };

var polygonWithTwoHoles =
    new GeoPolygon(outerRing, hole1, hole2);

```

![3 GeoPolygons](/images/geopolygons.png)

## Persisting GeoPoint Data
> **IMPORTANT:**
> Currently, you can only persist geospatial data. Geospatial data types *cannot* be persisted directly. For example, you
can't declare a property that is of type `GeoBox`.
>
> These types can only be used as arguments for geospatial queries.
>

If you want to persist `GeoPoint` data, it must conform to the
[GeoJSON spec](https://datatracker.ietf.org/doc/html/rfc7946).

### Creating a GeoJSON-Compatible Class
To create a class that conforms to the GeoJSON spec, you:

1. Create an embedded realm object (a class that inherits from
`IEmbeddedObject`).
2. At a minimum, add the two fields required by the GeoJSON spec: A field of type `IList<double>` that maps to a "coordinates" (case sensitive)
property in the realm schema.A field of type `string` that maps to a "type" property. The value of this
field must be "Point".

The following example shows an embedded class named "CustomGeoPoint" that is used
to persist GeoPoint data:

```csharp
public partial class CustomGeoPoint : IEmbeddedObject
{
    [MapTo("coordinates")]
    public IList<double> Coordinates { get; } = null!;

    [MapTo("type")]
    private string Type { get; set; } = "Point";

    public CustomGeoPoint(double latitude, double longitude)
    {
        Coordinates.Add(longitude);
        Coordinates.Add(latitude);
    }
}

```

### Using the Embedded Class
You then use the custom GeoPoint class in your realm model, as shown in the
following example:

```csharp
public partial class Company : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public CustomGeoPoint? Location { get; set; }

    public Company() { }
}

```

You then add instances of your class to realm just like any other Realm model:

```csharp
realm.WriteAsync(() =>
{
    realm.Add(new Company
    {
        Location = new CustomGeoPoint(47.68, -122.35)
    });
    realm.Add(new Company
    {
        Location = new CustomGeoPoint(47.9, -121.85)
    });
});

```

The following image shows the results of creating these two company objects.

![2 GeoPoints](/images/geopoints.png)

## Query Geospatial Data
To query against geospatial data, you can use the
`GeoWithin`
method, or you can use the `geoWithin` operator with RQL.
The `GeoWithin` method takes the "coordinates" property of an embedded object
that defines the point we're querying, and one of the geospatial shapes to
check if that point is contained within the shape.

> **NOTE:**
> The format for querying geospatial data is the same regardless the shape of
the geodata region.
>

The following example shows the difference between querying with the `GeoWithin`
method and RQL:

```csharp
var GeoWthinExample = realm.All<Company>()
    .Where(c => QueryMethods.GeoWithin(c.Location, circle1));

var RQLExample = realm.All<Company>()
    .Filter("Location geoWithin $0", circle2);

```

The following examples show querying against various shapes to return a list of
companies within the shape:

**GeoCircle**

```csharp
var companiesInCircle = realm.All<Company>()
    .Where(c => QueryMethods.GeoWithin(c.Location, circle1));

var companiesInSmallerCircle = realm.All<Company>()
    .Where(c => QueryMethods.GeoWithin(c.Location, circle2));

```

![Querying a GeoCircle example.](/images/geocircles-query.png)

**GeoBox**

```csharp
var companiesInBox1 = realm.All<Company>()
    .Where(c => QueryMethods.GeoWithin(c.Location, box1));

var companiesInBox2 = realm.All<Company>()
    .Where(c => QueryMethods.GeoWithin(c.Location, box2));

```

![Querying a GeoBox example.](/images/geoboxes-query.png)

**GeoPolygon**

```csharp
var companiesInBasicPolygon = realm.All<Company>()
    .Where(c => QueryMethods
       .GeoWithin(c.Location, basicPolygon));

var companiesInPolygon = realm.All<Company>()
    .Where(c => QueryMethods
       .GeoWithin(c.Location, polygonWithTwoHoles));

```

![Querying a GeoPolygon example.](/images/geopolygons-query.png)
