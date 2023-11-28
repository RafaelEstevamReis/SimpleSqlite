using Simple.DatabaseWrapper.Attributes;
using System;
using System.Drawing;

// Choose your example
Console.WriteLine("Choose one example");
//Test.Sample.FullCycle.run();
//Test.SampleWithExtensions.FullCycle.run();
//Test.Sample.IntPrimaryKeyExample.run();
//Test.Sample.SimpleTypeQuery.run();
//Test.Sample.DocumentStorage.run();
//Test.Sample.ConfigExample.run();

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
    public Color MyFavColor { get; set; }
    public decimal MyDecimalValue { get; set; }
    public double MyDoubleValue { get; set; }
    public float MyFloatValue { get; set; }
    public eIntEnum MyEnum { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Testing")]
    public int GetOnly => -1;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Testing")]
    public int SetOnly
    {
        set { _ = value; }
    }
    [Ignore]
    public int IgnoreMe { get; set; }
    public TimeSpan MyTimeSpan { get; set; }
}

