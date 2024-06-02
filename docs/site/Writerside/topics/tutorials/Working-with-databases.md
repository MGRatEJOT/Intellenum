# Working with databases

<note>
This topic is incomplete and is being improved.
</note>

Intellenum has converters and serializers for databases, including:

* Dapper
* EFCore
* [LINQ to DB](https://github.com/linq2db/linq2db)

They are controlled by the `Conversions` enum. The following specifies Newtonsoft.Json and System.Text.Json converters:

```c#
[Intellenum<int>(conversions: 
    Conversions.NewtonsoftJson | Conversions.SystemTextJson]
[Member("Standard", 1)]
[Member("Gold", 2)]
public partial class CustomerType;
```

If you don't want any conversions, then specify `Conversions.None`.

If you want your own conversion, then again specify none, and implement them yourself, just like any other type.  But be aware that even serializers will get the same compilation errors for `new` and `default` when trying to create Intellenums.

If you want to use Dapper, remember to register it—something like this:

```c#
SqlMapper.AddTypeHandler(new CustomerType.DapperTypeHandler());
```

See the examples folder for more information.
