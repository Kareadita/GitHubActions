using System.Collections.Generic;
using System.IO.Abstractions;
using Kavita.ReleaseTools.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Kavita.ReleaseTools.Configuration;

/// <summary>
/// Reads the release configuration from YAML, then layers the environment over it.
/// </summary>
public static class ConfigurationReader
{
    /// <exception cref="ExecutionException">
    /// The config file is missing, empty, or not valid YAML
    /// </exception>
    public static ReleaseConfiguration Read(IFileSystem fileSystem, string configPath)
    {
        var configuration = Deserialize(fileSystem, configPath);

        EnvironmentConfigurationBinder.Apply(configuration);

        return configuration;
    }

    /// <summary>
    /// Checks the root configuration. Settings belonging to a stage are checked by that stage.
    /// </summary>
    public static List<ValidationIssue> Validate(ReleaseConfiguration configuration) =>
        ValidationIssueFactory.FromAnnotations(nameof(ConfigurationReader), configuration);

    private static ReleaseConfiguration Deserialize(IFileSystem fileSystem, string configPath)
    {
        if (!fileSystem.File.Exists(configPath))
        {
            throw new ExecutionException($"Config file not found: {configPath}");
        }

        var yaml = fileSystem.File.ReadAllText(configPath);
        var deserializer = new DeserializerBuilder()
            .WithCaseInsensitivePropertyMatching()
            .Build();

        try
        {
            return deserializer.Deserialize<ReleaseConfiguration>(yaml)
                   ?? throw new ExecutionException($"Config file is empty: {configPath}");
        }
        catch (YamlException exception)
        {
            throw new ExecutionException($"Cannot parse {configPath}: {exception.Message}", exception);
        }
    }
}
