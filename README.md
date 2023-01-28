[![.NET Core](https://github.com/RafaelEstevamReis/SqliteWrapper/workflows/.NET%20Core/badge.svg)](https://github.com/RafaelEstevamReis/SimpleSqlite) 
[![NuGet](https://buildstats.info/nuget/Simple.Sqlite)](https://www.nuget.org/packages/Simple.Sqlite)

# Simple.Sqlite
A very simple Sqlite wrapper to plug spiders with it

Underlying library: [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite/)

## Compatibility

Huge compatibility, supports:
* NET 6.0
* NET 5.0
* NetCore 3.1
* Net Framework 4.6.1
* Net Standard 2.0
  * Net Core 2.0+
  * Mono 5.4+
  * Xamarin.iOS 10.14+
  * UWP 10.0.16299+
  * Unity 2018.1+

## Table of Contents

- [Simple.Sqlite](#simplesqlite)
  - [Compatibility](#compatibility)
  - [Table of Contents](#table-of-contents)
  - [Modules and Options](#modules-and-options)
  - [How to get started:](#how-to-get-started)
- [ConnectionFactory - Allow to use as Extensions](#connectionfactory---allow-to-use-as-extensions)
  - [How to use:](#how-to-use)
  - [What this module automates ?](#what-this-module-automates-)
    - [Auto fill parameters](#auto-fill-parameters)
    - [Migration](#migration)
  - [Using with Dependency Injection](#using-with-dependency-injection)
    - [Using with ASP .Net](#using-with-asp-net)
- [SqliteDB - A simplified Sqlite wrapper](#sqlitedb---a-simplified-sqlite-wrapper)
  - [How to use:](#how-to-use-1)
- [NoSqliteStorage - No-sql document storage](#nosqlitestorage---no-sql-document-storage)
  - [How to use:](#how-to-use-2)
  - [What this module automates ?](#what-this-module-automates--1)
- [ConfigurationDB - Configuration storage](#configurationdb---configuration-storage)
  - [How to use:](#how-to-use-3)
  - [What this module automates ?](#what-this-module-automates--2)
- [Backup](#backup)
- [Reutilizing types](#reutilizing-types)

## Modules and Options

There are three modules in this package
* Extension-based database manipulation
* [SqliteDB](#sqlitedb---sqlite-wrapper) -> Sqlite wrapper 
* No-sql document storage
* Configuration storage

## How to get started:

Install the [NuGet package](https://www.nuget.org/packages/Simple.Sqlite): `Install-Package Simple.Sqlite`

Look at the examples int the project [Test](https://github.com/RafaelEstevamReis/SqliteWrapper/tree/main/Test) and the [Samples folder](https://github.com/RafaelEstevamReis/SqliteWrapper/tree/main/Test/Sample)

Or follow any of examples bellow


# ConnectionFactory - Allow to use as Extensions

A Sqlite connection factory for `ISqliteConnection` to use with extension methods

## How to use:


~~~C#
using Simple.Sqlite.Extension;

// Create a new instance
using var cnn = Simple.Sqlite.ConnectionFactory.CreateConnection("myExtendedStuff.db");

// Create a DB Schema
cnn.CreateTables()
   .Add<MyData>()
   .Commit();

var d = new MyData()
{
    //fill your object
};
// call INSERT
cnn.Insert(d);

// use GetAll to retrieve all data
var allData = cnn.GetAll<MyData>();
// Use queries to get back data
var allBobs = cnn.Query<MyData>("SELECT * FROM MyData WHERE MyName = @name ", new { name = "bob" });
~~~


## What this module automates ?

### Auto fill parameters

This library provides a Query operation similar to **Dapper**, it can return a query as an Enumerable of your class

~~~C#
var allData = cnn.GetAll<MyData>();
~~~

And supports objects (even anonymous) as parameters 

~~~C#
var allBobs = cnn.Query<MyData>("SELECT * FROM MyData WHERE MyName = @name ", new { name = "bob" });
~~~

Also, it supports easy Insertion
~~~C#
var d = new MyData()
{
    //fill your object
};
// call INSERT
cnn.Insert(d);
~~~

And a VERY efficient, transaction based BulkInsertion
~~~C#
MyData[] lotsOfData = getLotsOfData();

// call INSERT
cnn.BulkInsert(lotsOfData);
~~~

_Tip: For multi-million insertion, 5k blocks are a good start point_

### Migration

This library has a very simple Migration tah can:
* Create new tables 
* Add columns to existing tables

To update your db schema just call CreateTables() and add your classes with `Add<T>` and then Commit()

~~~C#
// Create a connection...
using var cnn = ConnectionFactory.CreateConnection("myStuff.db");
// ... or new SqliteDB instance (see more below)
SqliteDB cnn = new SqliteDB("myStuff.db");


// Create a DB Schema
 var migrationResult = cnn.CreateTables()
                          .Add<MyData>()
                          .Commit();
~~~

A `TableCommitResult` will be returned with all changes made

This command will **not** migrate DATA only the schema

You can make changes on the table definition before it commits with:
~~~C#
cnn.CreateTables()
   .Add<MyData>()
      .ConfigureTable(t => { /* change last added table here */ })
   .Add<NextTable>()
   .Commit();
~~~

You can use Attributes on your properties to create columns marked with:
* PrimaryKey
* Allow Null
* Not Null
* Default Value
* Unique
~~~C#
// column tweak example
cnn.CreateTables()
  .Add<MyData>()
     .ConfigureTable(t => t["MyId"].IsUnique = true)
   .Commit();
~~~

Also,
* Int properties with PK Attribute is also created as Auto-Increment
* If neither Not-Null nor Allow-Null is applied, the library will assume by the attribute type

## Using with Dependency Injection

~~~C#
// Create a new instance
var db = ConnectionFactory.FromFile("myExtendedStuff.db");
// add to DI
services.AddSingleton(db);

// Execute schema Migration
using var cnn = db.GetConnection();
var result = cnn.CreateTables()
   .Add<MyData>()
   .Commit();

// result contains migration details
~~~

### Using with ASP .Net

~~~C#

[ApiController]
public class MyController : Controller
{
  private readonly ConnectionFactory db;
  private readonly ILogger logger;
  public AuthController(IDatabase db, ILogger<MyController> logger){
    this.db = db;
    this.logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetData(){
    using var cnn = db.GetConnection();
    return cnn.GetWhere<MyData>("SomeField", 1234).ToArray();
  }
}

~~~

# NoSqliteStorage - No-sql document storage

Need a fast to setup, no-installation no-sql database ? This module get you covered

Your data will be stored encoded (serialized) with BSON

## How to use:

~~~C#
// create a new instance
NoSqliteStorage db = new NoSqliteStorage("myStuff.db");
// build your complex type
MyData d = new MyData()
{
    MyId = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
    MyName = "My name is bob",
    MyBirthDate = DateTime.Now,
    MyUID = Guid.NewGuid(),
    MyWebsite = new Uri("http://example.com"),
    MyDecimalValue = 123.4M,
    MyDoubleValue = 456.7,
    MyFloatValue = 789.3f,
    MyEnum = MyData.eIntEnum.Zero,
};
// Store it serialized with BSON
db.Store(d.MyUID, d);
// simple call Retrieve<T> to get your data deserialized back
var d2 = db.Retrieve<MyData>(id);
~~~

## What this module automates ?

No-install, ready to use no-sql database
* No tables
* No migration
* no-nothing 'relational'

# ConfigurationDB - Configuration storage

Need to store some options or user settings ? This module will cover that

## How to use:

~~~C#
// Create a new instance
ConfigurationDB db = new ConfigurationDB("myStuff.db");

// store your data with Key-Category pair
db.SetConfig<string>("Font", "User.UI.FrontPanel", "Arial");
db.SetConfig<Color>("BackColor", "User.UI.FrontPanel", Color.Red);

// retrieve your data with "default option"
var myFont = db.GetConfig<string>("Font", "User.UI.FrontPanel", "Times");
var myBkg  = db.GetConfig<Color>("BackColor", "User.UI.FrontPanel", Color.Green);

// remove config
db.RemoveConfig("Font", "User.UI.FrontPanel");
~~~

## What this module automates ?

This automates various configuration save-set scenarios

Natively supported types:
* All C# "primitive" types
* string
* decimal
* DateTime
* TimeSpan
* Guid
* byte[]
* System.Drawing.Color

Primitive types is all types that type.IsPrimitive == true. Is all types you expect to be primitives: int, float, double, byte, etc...

Check all on [this MSDN article](https://docs.microsoft.com/en-us/dotnet/api/system.type.isprimitive?view=net-5.0)

# Backup

When a new SqliteDB instance is created a new backup of the database file is made. 

As NoSqliteStorage and ConfigurationDB internally uses SqliteDB, they also create a backup

First, the database file is copied and compressed as `*.bak.gz`
Later backups, the old `*.bak.gz` is renamed to `*.old.gz` and a new `*.bak.gz` is created

See an example with a database named `data.db`:
* On first execution, there is no file
  * a new empty `data.db` will be created
  * a new also empty `data.db.bak.gz` is created
* On later execution, 
  * the backup `data.db.bak.gz` will be renamed to `data.db.old.gz`
  * a new `data.db.bak.gz` will be created from `data.db`

Every execution, you have the previous version available as a backup

* To disable backups, you can use the static property as follows:
~~~C#
//this will disable the creation of backups of any posterior instances
SqliteDB.EnabledDatabaseBackup = false;
//this is specially useful when the database reaches a significant size
~~~

If you decided to use multiple instances with the same database file, you can avoid multiple backups by [reutilizing types](#reutilizing-types)

# Reutilizing types

If you already have a database and want to stick more stuff to it, you can simply:

* Already have a SqliteDB instance ?
~~~C#
var noSql = NoSqliteStorage.FromDB(mySqliteDB);
var cfg = ConfigurationDB.FromDB(mySqliteDB);
~~~

* Already have a NoSqliteStorage instance ?
~~~C#
var db = SqliteDB.FromDB(noSql);
var cfg = ConfigurationDB.FromDB(mySqliteDB);
~~~

* Already have a ConfigurationDB instance ?
~~~C#
var db = SqliteDB.FromDB(cfg);
var noSql = NoSqliteStorage.FromDB(cfg);
~~~
