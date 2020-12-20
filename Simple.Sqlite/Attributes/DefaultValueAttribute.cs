using System;

namespace Simple.Sqlite.Attributes
{
    public class DefaultValueAttribute : Attribute
    {
        public object DefaultValue { get; }
        public DefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}
