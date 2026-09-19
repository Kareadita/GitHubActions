using System;
using System.IO.Abstractions;
using Kavita.ReleaseTools.Models;
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

        return deserializer.Deserialize<ReleaseConfiguration>(yaml);
    }



}
