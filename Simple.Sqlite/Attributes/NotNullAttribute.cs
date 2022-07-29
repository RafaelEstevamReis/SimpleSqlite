using System;

namespace Simple.Sqlite.Attributes
{
    /// <summary>
    /// Specify that this column should not allow nulls
    /// </summary>
    [Obsolete("Use DatabaseWrapper.Attributes.NotNullAttribute instead", true)]
    public class NotNullAttribute : DatabaseWrapper.Attributes.NotNullAttribute
    { }
}
