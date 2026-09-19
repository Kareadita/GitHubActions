namespace Kavita.ReleaseTools.Configuration;

/// <summary>
/// Marks a configuration section the <see cref="EnvironmentConfigurationBinder"/> is allowed to walk
/// into, looking for <see cref="FromEnvironmentAttribute"/> properties.
/// </summary>
public interface IHasEnvironmentValues;
