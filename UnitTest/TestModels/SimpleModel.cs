namespace UnitTest.TestModels;

using Simple.DatabaseWrapper.Attributes;

public class SimpleModel
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; }
    public float Value { get; set; }
    public bool Enabled { get; set; }
}
