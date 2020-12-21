![.NET Core](https://github.com/RafaelEstevamReis/SqliteWrapper/workflows/.NET%20Core/badge.svg)
[![NuGet](https://buildstats.info/nuget/Simple.Sqlite)](https://www.nuget.org/packages/Simple.Sqlite)

# SqliteWrapper
A very simple Sqlite wrapper to plug spiders with it

- [SqliteWrapper](#sqlitewrapper)
  - [How to use:](#how-to-use)
- [What this library automates ?](#what-this-library-automates-)
  - [Auto fill parameters](#auto-fill-parameters)
  - [Migration](#migration)

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

# What this library automates ?

## Auto fill parameters

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

## Migration

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