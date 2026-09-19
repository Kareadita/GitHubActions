
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools;

public static class ValidationReport
{
    public static void Render(IReadOnlyList<ValidationIssue> issues)
    {
        AnsiConsole.Write(new Rule("[red]Validation failed[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Stage")
            .AddColumn("Message")
            .AddColumn("Solutions");

        foreach (var issue in issues)
        {
            table.AddRow(
                new Markup($"[yellow]{Markup.Escape(issue.StageName)}[/]"),
                new Markup($"[red]{Markup.Escape(issue.Message)}[/]"),
                new Markup(issue.Solutions.Count == 0
                    ? "[grey]—[/]"
                    : $"[green]{issue.Solutions.Count}[/]"));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        foreach (var issue in issues.Where(i =>
                     !string.IsNullOrWhiteSpace(i.ExtraInfo) || i.Solutions.Count > 0))
        {
            RenderDetail(issue);
        }
    }

    private static void RenderDetail(ValidationIssue issue)
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        grid.AddRow("[grey]Stage[/]",   $"[yellow]{Markup.Escape(issue.StageName)}[/]");
        grid.AddRow("[grey]Message[/]", $"[red]{Markup.Escape(issue.Message)}[/]");

        if (!string.IsNullOrWhiteSpace(issue.ExtraInfo))
        {
            grid.AddRow("[grey]Details[/]", Markup.Escape(issue.ExtraInfo));
        }

        if (issue.Solutions.Count > 0)
        {
            var bullets = string.Join("\n",
                issue.Solutions.Select(s => $"  [green]•[/] {Markup.Escape(s)}"));
            grid.AddRow("[grey]Solutions[/]", bullets);
        }

        AnsiConsole.Write(new Panel(grid)
            .Header($"[red]{Markup.Escape(issue.StageName)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red)
            .Padding(1, 0, 1, 0));

        AnsiConsole.WriteLine();
    }
}
