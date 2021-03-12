using System;

namespace Simple.Sqlite.Attributes
{
    /// <summary>
    /// Specify that this column should allow nulls
    /// </summary>
    [Obsolete("Use DatabaseWrapper.Attributes.AllowNullAttribute instead")]
    public class AllowNullAttribute : DatabaseWrapper.Attributes.AllowNullAttribute
    { }
}
