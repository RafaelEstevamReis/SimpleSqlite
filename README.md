[![.NET Core](https://github.com/RafaelEstevamReis/SqliteWrapper/workflows/.NET%20Core/badge.svg)](https://github.com/RafaelEstevamReis/SqliteWrapper) 
[![NuGet](https://buildstats.info/nuget/Simple.Sqlite)](https://www.nuget.org/packages/Simple.Sqlite)

# SqliteWrapper
A very simple Sqlite wrapper to plug spiders with it

- [SqliteWrapper](#sqlitewrapper)
  - [Modules and Options](#modules-and-options)
  - [How to get started:](#how-to-get-started)
- [SqliteDB - Sqlite wrapper](#sqlitedb---sqlite-wrapper)
  - [How to use:](#how-to-use)
  - [What this module automates ?](#what-this-module-automates-)
    - [Auto fill parameters](#auto-fill-parameters)
    - [Migration](#migration)
- [NoSqliteStorage - No-sql document storage](#nosqlitestorage---no-sql-document-storage)
  - [How to use:](#how-to-use-1)
  - [What this module automates ?](#what-this-module-automates--1)
- [ConfigurationDB - Configuration storage](#configurationdb---configuration-storage)
  - [How to use:](#how-to-use-2)
  - [What this module automates ?](#what-this-module-automates--2)

## Modules and Options

There are three modules in this package
* [SqliteDB](#sqlitedb---sqlite-wrapper) -> Sqlite wrapper 
* No-sql document storage
* Configuration storage

## How to get started:

Install the [NuGet package](https://www.nuget.org/packages/Simple.Sqlite): `Install-Package Simple.Sqlite`

Look at the examples int the project [Test](https://github.com/RafaelEstevamReis/SqliteWrapper/tree/main/Test) and the [Samples folder](https://github.com/RafaelEstevamReis/SqliteWrapper/tree/main/Test/Sample)

Or follow any of examples bellow

# SqliteDB - Sqlite wrapper

Sqlite database to store data in tables

## How to use:

~~~C#
// Create a new instance
SqliteDB db = new SqliteDB("myStuff.db");

// Create a DB Schema
db.CreateTables()
  .Add<MyData>()
  .Commit();

var d = new MyData()
{
    //fill your object
};
// call INSERT
db.Insert(d);

// use GetAll to retrieve all data
var allData = db.GetAll<MyData>();
// Use queries to get back data
var allBobs = db.ExecuteQuery<MyData>("SELECT * FROM MyData WHERE MyName = @name ", new { name = "bob" });
~~~

## What this module automates ?

### Auto fill parameters

This library provides a Query operation similar to **Dapper**, it can return a query as an Enumerable of your class

~~~C#
var allData = db.GetAll<MyData>();
~~~

And supports objects (even anonymous) as parameters 

~~~C#
var allBobs = db.ExecuteQuery<MyData>("SELECT * FROM MyData WHERE MyName = @name ", new { name = "bob" });
~~~

Also, it supports easy Insertion
~~~C#
var d = new MyData()
{
    //fill your object
};
// call INSERT
db.Insert(d);
~~~

And a VERY efficient, transaction based BulkInsertion
~~~C#
MyData[] lotsOfData = getLotsOfData();

// call INSERT
db.BulkInsert(lotsOfData);
~~~

_Tip: For multi-million insertion, 5k blocks are a good start point_

### Migration

This library has a very simple Migration tah can:
* Create new tables 
* Add columns to existing tables

To update your db schema just call CreateTables() and add your classes with `Add<T>` and then Commit()

~~~C#
// Create a new instance
SqliteDB db = new SqliteDB("myStuff.db");

// Create a DB Schema
 var migrationResult = db.CreateTables()
                         .Add<MyData>()
                         .Commit();
~~~

A `TableCommitResult` will be returned with all changes made

This command will **not** migrate DATA only the schema

You can make changes on the table definition before it commits with:
~~~C#
db.CreateTables()
  .Add<MyData>()
  .ConfigureTable(t => { /* change last added table here */ })
  .Add<NextTable>()
  .Commit();
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