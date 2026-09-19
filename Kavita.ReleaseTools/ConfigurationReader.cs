using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Linq;
using Kavita.ReleaseTools.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Kavita.ReleaseTools;

public static class ConfigurationReader
{

    public static ReleaseConfiguration Read(IFileSystem fs, string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: releasetools <config.yaml>");
            Environment.Exit(1);
        }

        var configPath = args[0];

        if (!fs.File.Exists(configPath))
        {
            Console.Error.WriteLine($"Config file not found: {configPath}");
            Environment.Exit(1);
        }

        var yaml = fs.File.ReadAllText(configPath);
        var deserializer = new DeserializerBuilder()
            .WithCaseInsensitivePropertyMatching()
            .Build();

        ReleaseConfiguration configuration;
        try
        {
            configuration = deserializer.Deserialize<ReleaseConfiguration>(yaml);
        }
        catch (YamlException ex)
        {
            FailureReport.Render(nameof(ConfigurationReader), ex);
            Environment.Exit(1);
            return null!; // Not reached
        }

        EnvironmentConfigurationBinder.Apply(configuration);

        Validate(configuration);

        return configuration;
    }

    private static void Validate(ReleaseConfiguration configuration)
    {
        System.ComponentModel.DataAnnotations.ValidationContext ctx = new(configuration);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(configuration, ctx, results, validateAllProperties: true);

        var issues = results.Select(r => new ValidationIssue
        {
            StageName = nameof(ConfigurationReader),
            Message = r.ErrorMessage ?? "Unknown validation error.",
            ExtraInfo = r.MemberNames.Any()
                ? string.Join(", ", r.MemberNames)
                : string.Empty,
            Solutions = []
        }).ToList();

        if (issues.Count <= 0) return;

        ValidationReport.Render(issues);
        Environment.Exit(1);
    }



}
