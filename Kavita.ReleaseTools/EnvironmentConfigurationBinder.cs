using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools;

/// <summary>
/// Fills properties marked with <see cref="FromEnvironmentAttribute"/> from the environment, after
/// the YAML configuration has been read.
/// </summary>
public static class EnvironmentConfigurationBinder
{
    public static void Apply(object? target, Func<string, string?>? getVariable = null)
    {
        if (target is null) return;

        Apply(target, getVariable ?? Environment.GetEnvironmentVariable, target.GetType().Name);
    }

    private static void Apply(object target, Func<string, string?> getVariable, string path)
    {
        foreach (var property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Indexers can't be read without arguments
            if (property.GetIndexParameters().Length > 0) continue;

            var propertyPath = $"{path}.{property.Name}";

            if (property.GetCustomAttribute<FromEnvironmentAttribute>() is { } attribute)
            {
                if (!property.CanWrite)
                {
                    throw new InvalidOperationException(
                        $"{propertyPath} is marked with FromEnvironment, but has no setter");
                }

                var value = getVariable(attribute.VariableName);
                if (string.IsNullOrWhiteSpace(value)) continue;

                property.SetValue(target, Convert(value, attribute.VariableName, propertyPath, property.PropertyType));
                continue;
            }

            if (property.GetValue(target) is not { } nested) continue;

            if (nested is IHasEnvironmentValues)
            {
                Apply(nested, getVariable, propertyPath);
            }
        }
    }

    private static object? Convert(string value, string variableName, string propertyPath, Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;

        try
        {
            return targetType.IsEnum
                ? Enum.Parse(targetType, value, ignoreCase: true)
                : TypeDescriptor.GetConverter(targetType).ConvertFromInvariantString(value);
        }
        catch (Exception e) when (e is FormatException or NotSupportedException or ArgumentException)
        {
            throw new InvalidOperationException(
                $"Cannot parse environment variable {variableName} ('{value}') as {targetType.Name} for {propertyPath}",
                e);
        }
    }
}
