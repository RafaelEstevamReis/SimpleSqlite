using System;
using Simple.Sqlite;


SqliteDB db = new SqliteDB("myStuff.db");
Console.WriteLine($"Database is at {db.DatabaseFileName}");

// Create a DB Schema
db.CreateTables()
  .Add<MyData>()
  .Commit();

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
Console.WriteLine($"New data inserted: Id={d.MyId}");
db.Insert(d);

// get all data
var allData = db.GetAll<MyData>();

Console.WriteLine("All data:");
foreach (var rowData in allData)
{
    Console.WriteLine($" > {rowData.MyId} {rowData.MyName}");
}

//get "bob" data
var bob = db.ExecuteQuery<MyData>("SELECT * FROM MyData WHERE MyName LIKE @name ", new { name = "%bob%" });
Console.WriteLine("All bob data:");
foreach (var rowData in allData)
{
    Console.WriteLine($" > {rowData.MyId} {rowData.MyName}");
}


public class MyData
{
    public enum eIntEnum
    {
        Zero = 0,
        One = 1,
        Two = 2,
    }

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

