using System;

namespace Simple.Sqlite.Attributes
{
    /// <summary>
    /// Specify that this column is PrimaryKey
    /// </summary>
    [Obsolete("Use DatabaseWrapper.Attributes.PrimaryKeyAttribute instead")]
    public class PrimaryKeyAttribute : DatabaseWrapper.Attributes.PrimaryKeyAttribute
    { }
}
