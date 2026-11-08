namespace Simple.Sqlite.Helpers;

using Microsoft.Data.Sqlite;
using Simple.Sqlite.Attributes;
using Simple.DatabaseWrapper.Attributes;
using Simple.DatabaseWrapper.Helpers;
using Simple.DatabaseWrapper.TypeReader;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Collections.Concurrent;

internal class HelperFunctions
{
    internal static bool handleGuidAsByteArray = true;

    private static readonly ConcurrentDictionary<Type, bool> epochTypes = new();
    internal static bool IsEpochType(Type type)
                                => epochTypes.GetOrAdd(type, static t => t.IsDefined(typeof(DateTimePolicyAttribute), false)
                                              && Attribute.GetCustomAttribute(t, typeof(DateTimePolicyAttribute), false)
                                                           is DateTimePolicyAttribute { Policy: DateTimePolicyAttribute.Policies.EpochMs });

    internal static string BuildInsertSql<T>(ReaderCachedCollection typeCollection, OnConflict resolution, string tableName)
    {
        var info = typeCollection.GetInfo<T>();

        var names = getNames(info, !info.IsAnonymousType);
        return BuildInsertSql(names, resolution, tableName);
    }

    internal static string BuildInsertSql(IEnumerable<string> names, OnConflict resolution, string tableName)
    {
        var fields = string.Join(",", names.Select(QuoteName));
        var values = string.Join(",", names.Select(n => $"@{n}"));

        if (resolution == OnConflict.Abort)
        {
            return $"INSERT INTO {QuoteName(tableName)} ({fields}) VALUES ({values}); SELECT last_insert_rowid();";
        }
        else
        {
            string txtConflict = resolution switch
            {
                OnConflict.RollBack => "ROLLBACK",
                OnConflict.Fail => "FAIL",
                OnConflict.Ignore => "IGNORE",
                OnConflict.Replace => "REPLACE",
                _ => throw new ArgumentException($"Invalid resolution: {resolution}"),
            };

            return $"INSERT OR {txtConflict} INTO {QuoteName(tableName)} ({fields}) VALUES ({values}); SELECT last_insert_rowid();";
        }
    }

    internal static void FillParameters(SqliteCommand cmd, object? parameters, ReaderCachedCollection typeCollection)
    {
        if (parameters == null) return;

        var type = GetCommandParameters(parameters, typeCollection);

        foreach (var p in type)
        {
            cmd.Parameters.AddWithValue(p.name, p.value);
        }
    }

    internal static IEnumerable<(string name, object value)> GetCommandParameters(object? parameters, ReaderCachedCollection typeCollection)
    {
        if (parameters == null) return [];

        return typeCollection
            .GetInfo(parameters.GetType())
            .Items
            .Where(p => p.CanRead)
            .Select(p =>
            {
                var value = TypeHelper.ReadParamValue(p, parameters, handleGuidAsByteArray);
                adjustInsertValue(ref value, p, parameters);

                if (value is null) value = DBNull.Value;

                return (p.Name, value);
            });
    }

    internal static IEnumerable<string> GetParametersNames(object parameters, ReaderCachedCollection typeCollection)
    {
        return typeCollection.GetInfo(parameters.GetType())
                   .Items
                   .Where(p => p.CanRead)
                   .Select(p => p.Name);
    }

    internal static string[] getSchemaColumns(IDataReader reader)
    {
        return Enumerable.Range(0, reader.FieldCount)
                         .Select(idx => reader.GetName(idx))
                         .ToArray();
    }

    internal static string QuoteName(string name)
    {
        // rowid pseudo-columns must stay unquoted to keep their special meaning
        if (string.Equals(name, "rowid", StringComparison.OrdinalIgnoreCase)) return name;
        if (string.Equals(name, "_rowid_", StringComparison.OrdinalIgnoreCase)) return name;
        if (string.Equals(name, "oid", StringComparison.OrdinalIgnoreCase)) return name;

        return "\"" + name.Replace("\"", "\"\"") + "\"";
    }

    private static void adjustInsertValue(ref object value, TypeItemInfo p, object parameters)
    {
        if (value is Uri uri)
        {
            value = uri.ToString();
        }

        if (p.Type.IsEnum)
        {
            var policy = p.GetAttribute<EnumPolicyAttribute>(DatabaseWrapper.ColumnAttributes.Other);
            if (policy != null && policy.Policy == EnumPolicyAttribute.Policies.AsText)
            {
                value = value.ToString() ?? default!;
            }
        }

        if (value is DateTime dateTime)
        {
            if (IsEpochType(parameters.GetType()))
            {
                // Strict write: Local is converted to Utc first, Unspecified is written as-is (assumed Utc wall time)
                var utc = dateTime.Kind == DateTimeKind.Local ? dateTime.ToUniversalTime() : dateTime;
                // TimeSpan.Zero: Unspecified must be taken as UTC wall time, not local offset
                value = new DateTimeOffset(utc, TimeSpan.Zero).ToUnixTimeMilliseconds();
            }
            else
            {
                // Local      -> converted to Utc
                // Utc        -> persisted with a 'Z' suffix
                // Unspecified-> persisted as-is, without suffix (backwards compatible)
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    value = dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF'Z'", CultureInfo.InvariantCulture);
                }
                else if (dateTime.Kind == DateTimeKind.Local)
                {
                    value = dateTime.ToUniversalTime()
                                     .ToString("yyyy-MM-dd HH:mm:ss.FFFFFF'Z'", CultureInfo.InvariantCulture);
                }
            }
        }

        // Process Only Primary Keys
        if (!p.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey)) return;

        //  Check for INT or LONGs equals to zero
        if (p.Type == typeof(int))
        {
            if ((int)value != 0) return;
            value = default!;
        }
        else if (p.Type == typeof(long))
        {
            if ((long)value != 0) return;
            value = default!;
        }
        else if (p.Type == typeof(Guid))
        {
            // If Guid <=> Guid, fill all EMPTY ones
            if (value is Guid guid)
            {
                if (guid != Guid.Empty) return;
                value = Guid.NewGuid();
                // Write back to the object
                p.SetValue(parameters, value);
            }
            // If byte[] <=> Guid, fill all [0,0,0,..]
            if (value is byte[] b)
            {
                // If all zero, initialize
                if (isAllZeros(b))
                {
                    var newGuid = Guid.NewGuid();
                    value = newGuid.ToByteArray();
                    // Write back to the object
                    p.SetValue(parameters, newGuid);
                }
            }
        }
    }

    private static IEnumerable<string> getNames(TypeInfo type, bool needWrite)
    {
        return type.Items
                   .Where(o => !o.Is(DatabaseWrapper.ColumnAttributes.Ignore))
                   .Where(o => o.CanRead)
                   .Where(o => !needWrite || o.CanWrite) // Be careful with NOTs and ORs
                   .Select(o => o.Name);
    }

    private static bool isAllZeros(byte[] array)
    {
        if (array == null || array.Length == 0) return true;

#if NET8_0_OR_GREATER
        return array.AsSpan().IndexOfAnyExcept((byte)0) == -1;
#else
        return Array.TrueForAll(array, element => element == 0);
#endif
    }
}