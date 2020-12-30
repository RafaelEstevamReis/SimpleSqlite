using System;
using Simple.Sqlite.Attributes;

// Choose your example
//Test.Sample.FullCycle.run();
//Test.Sample.SimpleTypeQuery.run();
//Test.Sample.DocumentStorage.run();

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

