using System;
using Kavita.ReleaseTools.Models;
using Spectre.Console;

namespace Kavita.ReleaseTools;

/// <summary>
/// Reports a stage that could not finish, so a failed release ends with something readable instead
/// of an unhandled exception
/// </summary>
public static class FailureReport
{
    public static void Render(string stageName, Exception exception)
    {
        AnsiConsole.Write(new Rule("[red]Release failed[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        grid.AddRow("[grey]Stage[/]", $"[yellow]{Markup.Escape(stageName)}[/]");

        // An ExecutionException is raised by this tool and says what went wrong, anything else is a
        // surprise and needs its type to be identifiable
        if (exception is not ExecutionException)
        {
            grid.AddRow("[grey]Exception[/]", $"[red]{Markup.Escape(exception.GetType().Name)}[/]");
        }

        grid.AddRow("[grey]Reason[/]", $"[red]{Markup.Escape(exception.Message)}[/]");

        AnsiConsole.Write(new Panel(grid)
            .Header($"[red]{Markup.Escape(stageName)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red)
            .Padding(1, 0, 1, 0));

        if (exception is not ExecutionException)
        {
            AnsiConsole.WriteException(exception, ExceptionFormats.ShortenPaths);
        }

        AnsiConsole.WriteLine();
    }
}
