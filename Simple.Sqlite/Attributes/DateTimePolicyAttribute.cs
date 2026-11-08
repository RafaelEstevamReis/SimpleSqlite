namespace Simple.Sqlite.Attributes;

using System;

/// <summary>
/// Specify DateTime handling policy
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DateTimePolicyAttribute : Attribute
{
    /// <summary>
    /// Available policies
    /// </summary>
    public enum Policies
    {
        /// <summary>
        /// Stored as ISO-8601 text (default behavior)
        /// </summary>
        Text = 0,
        /// <summary>
        /// Stored as Unix epoch in milliseconds (UTC)
        /// </summary>
        EpochMs = 1,
    }
    /// <summary>
    /// Current Policy
    /// </summary>
    public Policies Policy { get; set; }
    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="policy">Policy to be used</param>
    public DateTimePolicyAttribute(Policies policy)
    {
        Policy = policy;
    }
}
