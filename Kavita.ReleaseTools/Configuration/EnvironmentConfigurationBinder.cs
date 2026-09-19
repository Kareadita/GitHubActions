using System;
using System.ComponentModel;
using System.Reflection;

namespace Kavita.ReleaseTools.Configuration;

/// <summary>
/// Fills properties marked with <see cref="FromEnvironmentAttribute"/> from the environment, after
/// the YAML configuration has been read.
/// </summary>
public static class EnvironmentConfigurationBinder
{
    public static void Apply(object? target, Func<string, string?>? getVariable = null)
    {
        if (target is null) return;

        Bind(target, getVariable ?? Environment.GetEnvironmentVariable, target.GetType().Name);
    }

    private static void Bind(object target, Func<string, string?> getVariable, string path)
    {
        foreach (var property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Indexers can't be read without arguments
            if (property.GetIndexParameters().Length > 0) continue;

            var propertyPath = $"{path}.{property.Name}";

            if (TryBindFromEnvironment(target, property, propertyPath, getVariable)) continue;

            if (property.GetValue(target) is IHasEnvironmentValues nested)
            {
                Bind(nested, getVariable, propertyPath);
            }
        }
    }

    /// <summary>
    /// Binds the property when it carries <see cref="FromEnvironmentAttribute"/>.
    /// </summary>
    /// <returns>
    /// Whether the property was an environment value, so the caller knows not to look for nested
    /// sections inside it.
    /// </returns>
    private static bool TryBindFromEnvironment(object target, PropertyInfo property, string propertyPath,
        Func<string, string?> getVariable)
    {
        if (property.GetCustomAttribute<FromEnvironmentAttribute>() is not { } attribute) return false;

        if (!property.CanWrite)
        {
            throw new InvalidOperationException(
                $"{propertyPath} is marked with FromEnvironment, but has no setter");
        }

        var value = getVariable(attribute.VariableName);
        if (string.IsNullOrWhiteSpace(value)) return true;

        property.SetValue(target, ConvertValue(value, attribute.VariableName, propertyPath, property.PropertyType));
        return true;
    }

    private static object? ConvertValue(string value, string variableName, string propertyPath, Type type)
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
