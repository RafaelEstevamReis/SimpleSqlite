using System;

namespace Simple.Sqlite.Attributes
{
    /// <summary>
    /// Specify default value for this column
    /// </summary>
    public class DefaultValueAttribute : Attribute
    {
        /// <summary>
        /// Default value specified
        /// </summary>
        public object DefaultValue { get; }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="defaultValue"></param>
        public DefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}
