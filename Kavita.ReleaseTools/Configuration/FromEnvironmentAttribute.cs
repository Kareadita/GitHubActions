using System;

namespace Kavita.ReleaseTools.Configuration;

/// <summary>
/// Loads the property from an environment variable. When the variable holds a non-empty value it
/// takes precedence over the value in the YAML, which allows secrets to stay out of the release
/// configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FromEnvironmentAttribute(string variableName) : Attribute
{
    /// <summary>
    /// Environment variable to read
    /// </summary>
    public string VariableName { get; } = variableName;
}
