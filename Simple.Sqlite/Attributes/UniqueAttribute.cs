﻿using System;

namespace Simple.Sqlite.Attributes
{
    /// <summary>
    /// Specify that this column should only have unique values
    /// </summary>
    [Obsolete("Use DatabaseWrapper.Attributes.UniqueAttribute instead")]
    public class UniqueAttribute : DatabaseWrapper.Attributes.UniqueAttribute
    { }
}
