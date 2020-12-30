using System;
using System.Linq;
using Simple.Sqlite;
using Simple.Sqlite.Attributes;

SqliteDB db = new SqliteDB("myStuff.db");
Console.WriteLine($"Database is at {db.DatabaseFileName}");

// Create a DB Schema
db.CreateTables()
  .Add<MyData>()
  .Commit();


var allStrings = db.ExecuteQuery<string>("SELECT MyName FROM MyData", null).ToArray();


var d = new MyData()
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
Console.WriteLine($"New data inserted: Id={d.MyId} MyUID={d.MyUID}");
db.Insert(d);

// get all data
var allData = db.GetAll<MyData>();

Console.WriteLine("All data:");
foreach (var rowData in allData)
{
    Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
}

//get "bob" data
var bobs = db.ExecuteQuery<MyData>("SELECT * FROM MyData WHERE MyName LIKE @name ", new { name = "%bob%" });
Console.WriteLine("All bob data:");
foreach (var rowData in bobs)
{
    Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
}

// change frst bob
var firstBob = bobs.First();
firstBob.MyName = "Changed bob";
db.InsertOrReplace(firstBob);
// show all data again
Console.WriteLine("All data:");
foreach (var rowData in allData)
{
    Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
}


public class MyData
{
    public enum eIntEnum
    {
        Zero = 0,
        One = 1,
        Two = 2,
    }

    [PrimaryKey]
    public Guid MyUID { get; set; }
    public int MyId { get; set; }
    public string MyName { get; set; }
    public Uri MyWebsite { get; set; }
    public DateTime MyBirthDate { get; set; }
    public decimal MyDecimalValue { get; set; }
    public double MyDoubleValue { get; set; }
    public float MyFloatValue { get; set; }
    public eIntEnum MyEnum { get; set; }
}

