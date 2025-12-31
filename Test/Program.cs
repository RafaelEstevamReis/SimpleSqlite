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
    public enum eTextEnum
    {
        Apples,
        Bananas,
        Oranges,
    }

    [PrimaryKey]
    public Guid MyUID { get; set; }
    [Index("ixMyData_MyId")]
    [Index("ixMyData_Ordered", 3)]
    public int MyId { get; set; }
    [Index("ixMyData_Ordered", 0)]
    public string MyName { get; set; }
    [Index("ixMyData_Ordered", 5)]
    public Uri MyWebsite { get; set; }
    public DateTime MyBirthDate { get; set; }
    public TimeSpan MyTimeSpan { get; set; }
    public Color MyFavColor { get; set; }
    public decimal MyDecimalValue { get; set; }
    public decimal? MyDecimalOptionalValue { get; set; }
    public double MyDoubleValue { get; set; }
    public float MyFloatValue { get; set; }
    [EnumPolicy(EnumPolicyAttribute.Policies.AsNumber)]
    public eIntEnum MyEnum { get; set; }
    [EnumPolicy(EnumPolicyAttribute.Policies.AsText)]
    public eTextEnum MyTextEnum { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Testing")]
    public int GetOnly => -1;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Testing")]
    public int SetOnly
    {
        set { _ = value; }
    }
    [Ignore]
    public int IgnoreMe { get; set; }
}

