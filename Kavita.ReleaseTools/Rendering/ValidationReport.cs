using System.Collections.Generic;
using System.Linq;
using Kavita.ReleaseTools.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Kavita.ReleaseTools.Rendering;

/// <summary>
/// Reports everything the configuration and the stages found wrong, in one report
/// </summary>
public static class ValidationReport
{
    public static void Render(IReadOnlyList<ValidationIssue> issues)
    {
        ReportPanel.WriteHeading("Validation failed");
        AnsiConsole.Write(BuildSummary(issues));
        AnsiConsole.WriteLine();

        foreach (var issue in issues.Where(HasDetail))
        {
            RenderDetail(issue);
        }
    }

    private static Table BuildSummary(IReadOnlyList<ValidationIssue> issues)
    {
        var anySolutions = issues.Any(i => i.Solutions.Count > 0);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Stage")
            .AddColumn("Message");

        if (anySolutions)
            table = table.AddColumn("Solutions");

        foreach (var issue in issues)
        {
            List<IRenderable> columns = [
                new Markup($"[yellow]{Markup.Escape(issue.StageName)}[/]"),
                new Markup($"[red]{Markup.Escape(issue.Message)}[/]")
            ];
            if (anySolutions)
            {
                columns.Add(new Markup(issue.Solutions.Count == 0
                    ? "[grey]—[/]"
                    : $"[green]{issue.Solutions.Count}[/]"));
            }

            table.AddRow([.. columns]);
        }

        return table;
    }

    private static void RenderDetail(ValidationIssue issue)
    {
        var grid = ReportPanel.DetailGrid()
            .AddRow("[grey]Stage[/]",   $"[yellow]{Markup.Escape(issue.StageName)}[/]")
            .AddRow("[grey]Message[/]", $"[red]{Markup.Escape(issue.Message)}[/]");

        if (!string.IsNullOrWhiteSpace(issue.ExtraInfo))
        {
            grid.AddRow("[grey]Details[/]", Markup.Escape(issue.ExtraInfo));
        }

        if (issue.Solutions.Count > 0)
        {
            var bullets = string.Join("\n",
                issue.Solutions.Select(solution => $"  [green]•[/] {Markup.Escape(solution)}"));
            grid.AddRow("[grey]Solutions[/]", bullets);
        }

        AnsiConsole.Write(ReportPanel.Wrap(issue.StageName, grid));
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Only the issues carrying more than the summary table shows get a panel of their own
    /// </summary>
    private static bool HasDetail(ValidationIssue issue) =>
        !string.IsNullOrWhiteSpace(issue.ExtraInfo) || issue.Solutions.Count > 0;
}
