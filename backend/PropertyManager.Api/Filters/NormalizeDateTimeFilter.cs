using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PropertyManager.Api.Filters;

/// <summary>
/// An action filter that normalizes DateTime values found in action arguments to UTC (or sets Kind=Utc)
/// to avoid Npgsql "Kind=Unspecified" errors when writing/reading timestamptz columns.
/// It mutates DateTime and Nullable<DateTime> values on the action arguments and their nested properties.
/// </summary>
public class NormalizeDateTimeFilter : IAsyncActionFilter
{
    private const int MaxDepth = 8;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var visited = new HashSet<object>();
        foreach (var key in context.ActionArguments.Keys.ToList())
        {
            var arg = context.ActionArguments[key];
            if (arg is null) continue;
            Normalize(arg, visited, 0);
        }

        await next();
    }

    private void Normalize(object? obj, HashSet<object> visited, int depth)
    {
        if (obj is null) return;
        if (depth > MaxDepth) return;

        // Avoid primitive/simple types
        var type = obj.GetType();
        if (type.IsPrimitive || type == typeof(string) || type.IsEnum) return;

        // Protect against cycles
        if (!type.IsValueType)
        {
            if (visited.Contains(obj)) return;
            visited.Add(obj);
        }

        // If it's a DateTime boxed in object (unlikely here) - handle explicitly
        if (obj is DateTime boxedDt)
        {
            // Nothing to set here because boxed value can't be changed in-place
            return;
        }

        // If it's a collection, handle arrays and indexable collections specially so we can replace DateTime elements
        if (obj is IEnumerable enumerable && !(obj is IDictionary))
        {
            var objType = obj.GetType();

            // Arrays -> can set elements via SetValue
            if (objType.IsArray)
            {
                var arr = (Array)obj;
                var elemType = objType.GetElementType();
                if (elemType == typeof(DateTime) || elemType == typeof(DateTime?))
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var val = arr.GetValue(i);
                        if (val is DateTime dt)
                        {
                            var normalized = NormalizeDateTime(dt);
                            if (normalized != dt) arr.SetValue(normalized, i);
                        }
                        else if (val is DateTime?)
                        {
                            var nullable = (DateTime?)val;
                            if (nullable.HasValue)
                            {
                                var normalized = NormalizeDateTime(nullable.Value);
                                if (normalized != nullable.Value) arr.SetValue((DateTime?)normalized, i);
                            }
                        }
                    }
                    return;
                }

                // Not a DateTime array, recurse into elements
                foreach (var item in arr)
                {
                    if (item is null) continue;
                    Normalize(item, visited, depth + 1);
                }
                return;
            }

            // Handle indexable generic lists or types exposing Count/Length and an indexer 'Item'
            var countProp = objType.GetProperty("Count") ?? objType.GetProperty("Length");
            var itemProp = objType.GetProperty("Item", new Type[] { typeof(int) });
            if (countProp != null && itemProp != null)
            {
                var count = (int?)countProp.GetValue(obj) ?? 0;
                var elemType = itemProp.PropertyType;
                if (elemType == typeof(DateTime) || elemType == typeof(DateTime?))
                {
                    for (int i = 0; i < count; i++)
                    {
                        var val = itemProp.GetValue(obj, new object[] { i });
                        if (val is DateTime dt)
                        {
                            var normalized = NormalizeDateTime(dt);
                            if (normalized != dt) itemProp.SetValue(obj, normalized, new object[] { i });
                        }
                        else if (val is DateTime?)
                        {
                            var nullable = (DateTime?)val;
                            if (nullable.HasValue)
                            {
                                var normalized = NormalizeDateTime(nullable.Value);
                                if (normalized != nullable.Value) itemProp.SetValue(obj, (DateTime?)normalized, new object[] { i });
                            }
                        }
                    }
                    return;
                }

                // Not a DateTime element type, recurse into elements
                for (int i = 0; i < count; i++)
                {
                    var val = itemProp.GetValue(obj, new object[] { i });
                    if (val is null) continue;
                    Normalize(val, visited, depth + 1);
                }
                return;
            }

            // Fallback: iterate items and recurse
            foreach (var item in enumerable)
            {
                if (item is null) continue;
                Normalize(item, visited, depth + 1);
            }
            return;
        }

        // Inspect properties
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite);

        foreach (var prop in props)
        {
            try
            {
                var propType = prop.PropertyType;
                var value = prop.GetValue(obj);
                if (value is null) continue;

                if (propType == typeof(DateTime))
                {
                    var current = (DateTime)value;
                    var normalized = NormalizeDateTime(current);
                    if (current != normalized)
                        prop.SetValue(obj, normalized);
                }
                else if (propType == typeof(DateTime?))
                {
                    var current = (DateTime?)value;
                    if (current.HasValue)
                    {
                        var normalized = NormalizeDateTime(current.Value);
                        if (normalized != current.Value)
                            prop.SetValue(obj, (DateTime?)normalized);
                    }
                }
                else if (!propType.IsPrimitive && propType != typeof(string))
                {
                    // Recurse into complex types
                    Normalize(value, visited, depth + 1);
                }
            }
            catch
            {
                // Ignore reflection errors and continue; normalization is best-effort
            }
        }
    }

    private static DateTime NormalizeDateTime(DateTime dt)
    {
        // If unspecified, set Kind=Utc without changing the moment in time (preserve value)
        if (dt.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        // If local, convert to UTC
        if (dt.Kind == DateTimeKind.Local)
            return dt.ToUniversalTime();

        return dt; // already Utc
    }
}
