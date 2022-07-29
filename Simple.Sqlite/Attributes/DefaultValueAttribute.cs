using System;

namespace Simple.Sqlite.Attributes
{
    /// <summary>
    /// Specify default value for this column
    /// </summary>
    [Obsolete("Use DatabaseWrapper.Attributes.DefaultValueAttribute instead", true)]
    public class DefaultValueAttribute : DatabaseWrapper.Attributes.DefaultValueAttribute
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="defaultValue"></param>
        public DefaultValueAttribute(object defaultValue)
            :base(defaultValue)
        { }
    }
}
