# HigherLogics.Google.Datastore

A convention-based Google Datastore entities to POCO auto mapper suitable
for small to medium sized projects:

    using HigherLogics.Google.Datastore;

    public class Foo
    {
        [Key]
        public long Baz { get; set; }
        public string Bar { get; set; }
    }
    ...
    var db = new Google.Cloud.Datastore.V1.DatastoreDb("MyProject");
	var fooId = db.Insert(new Foo { Bar = "Hello World!" });
	var foo = db.Lookup(fooId, new Foo());
    // returns: { Baz : <some long id>, Bar : "hello world!" }

It uses the standard attributes in the `System.ComponentModel.DataAnnotations`
namespace to designate the entity keys.

# Customizing Entity fields

You can specify custom name for fields using the `EntityFieldAttribute`  

    public class Foo
    {
        [Key]
        public long Baz { get; set; }
        
        [EntityField("__bar")]
        public string Bar { get; set; }
    }

This can be helpful in order to working with legacy databases or 
protect your data from property renames.


# Custom Value Conversions

Mapping should work for most built in CLR types, including all primitive
types, arrays, dates and nested entities. You can specify any missing
value conversions as follows:

    Mapper.Convert(from: (Value v) => ...create T,
                   to:   (T obj) => ...create Value);

# Integration with Existing Data

By default the full type name is used as the entity kind. To support
integration with existing data sets, you can specify the kind to use
for a given type:

    Mapper.Kind<Foo>("Books");

# Query Extensions

You can create a query using one of the following methods:

    var query = new Query(Mapper.Kind<T>())

Or:

	var db = new Google.Cloud.Datastore.V1.DatastoreDb("MyProject");
	...
	var query = db.CreateQuery<T>();

Result sets from datastore return a sequence of untyped entities which
you can easily convert to a typed sequence as follows:

    var results = db.RunQuery(query)
                    .Entities<Foo>();

Filters can be expressed using the following: 

    Filter<Foo>.Property(x => x.Bar, "something", Operator.Equal);
    
or

    Filter<Foo>.Equal(x => x.Bar, "something");
    Filter<Foo>.GreaterThan(x => x.Bar, 1);
    Filter<Foo>.GreaterOrEqualThan(x => x.Bar, 1);
    ...



# Performance Optimizations

By default this library calls Activator.CreateInstance<T>() to construct all
entity types but this is known to be very inefficient. For maximum
performance, I recommend that you override the default constructor for all
of your entity types:

    Mapper.Constructor<Foo>(() => new Foo());

This is because I wanted to support environments that don't permit code
generation, and code generation is the only automatic way to efficiently
invoke constructors.

However, the IEntityMapper interface can support an implementation that uses
System.Reflection.Emit or LINQ expressions, so you can replace the default
mapping backend as follows:

    public class YourCustomMapper : IEntityMapper { ... }

    Mapper.Default = new YourCustomMapper();

I will probably add a LINQ expression tree mapper at some point in a separate
assembly.

# Limitations

 * associations to foreign entities must be designated by a property of type
   Key as with the usual Datastore API, or the special FK<T> wrapper this library
   provides, and they must be manually loaded, inserted, etc. Any nested
   reference or value types will be saved as embedded entities. Supporting
   proper FK relations starts needing ORM-like features, requiring session-level map
   of keys to/from entities, transparent loading of associations and
   collections, and other features that can drastically impact performance
 * only tuples and value tuples up to 4 arguments are supported out of the box,
   as any more than that and you should create a class or struct

# Future Work

 * LINQ query interface
 * add something like [EntityField(string name)] to permit customizing the
   entity field names, which will make it easier to integrate with existing
   data