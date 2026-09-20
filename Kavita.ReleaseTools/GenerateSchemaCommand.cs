using System;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using Json.Schema.Generation.DataAnnotations;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools;

public static class GenerateSchemaCommand
{
    public static Command Build()
    {
        var outputOption = new Option<FileInfo>("--output", "-o")
        {
            Description = "Where to write the generated schema",
            Required = true
        };

        var command = new Command("generate-schema", "Generates the JSON schema")
        {
            outputOption
        };

        command.SetAction(parseResult =>
        {
            var output = parseResult.GetValue(outputOption)!;
            GenerateSchema(output);
            return 0;
        });

        return command;
    }

    private static void GenerateSchema(FileInfo output)
    {
        DataAnnotationsSupport.AddDataAnnotations();

        var config = new SchemaGeneratorConfiguration();
        var xmlPath = Path.Combine(AppContext.BaseDirectory, "Kavita.ReleaseTools.xml");
        config.RegisterXmlCommentFile<ReleaseConfiguration>(xmlPath);

        var schema = new JsonSchemaBuilder().FromType<ReleaseConfiguration>(config).Build();
        var json = JsonSerializer.Serialize(schema, JsonSerializerOptions);

        output.Directory?.Create();
        File.WriteAllText(output.FullName, json);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };
}
