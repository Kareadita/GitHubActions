using System;
using Kavita.ReleaseTools.Models;
using Spectre.Console;

namespace Kavita.ReleaseTools.Rendering;

public static class FailureReport
{
    public static void Render(string stageName, Exception exception)
    {
        ReportPanel.WriteHeading("Release failed");
        AnsiConsole.Write(BuildPanel(stageName, exception));
        AnsiConsole.WriteException(exception, ExceptionFormats.ShortenPaths);
        AnsiConsole.WriteLine();
    }

    private static Panel BuildPanel(string stageName, Exception exception)
    {
        var grid = ReportPanel.DetailGrid()
            .AddRow("[grey]Stage[/]", $"[yellow]{Markup.Escape(stageName)}[/]");

        if (exception is not ExecutionException)
        {
            grid.AddRow("[grey]Exception[/]", $"[red]{Markup.Escape(exception.GetType().Name)}[/]");
        }

        grid.AddRow("[grey]Reason[/]", $"[red]{Markup.Escape(exception.Message)}[/]");

        return ReportPanel.Wrap(stageName, grid);
    }
}
