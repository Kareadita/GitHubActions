using Spectre.Console;

namespace Kavita.ReleaseTools.Rendering;

/// <summary>
/// The presentation both reports share: a heading rule, and a two column detail grid wrapped in a
/// red panel.
/// </summary>
public static class ReportPanel
{
    public static void WriteHeading(string heading)
    {
        AnsiConsole.Write(new Rule($"[red]{Markup.Escape(heading)}[/]").LeftJustified());
        AnsiConsole.WriteLine();
    }

    public static Grid DetailGrid() => new Grid()
        .AddColumn(new GridColumn().NoWrap().PadRight(2))
        .AddColumn();

    public static Panel Wrap(string header, Grid grid) => new Panel(grid)
        .Header($"[red]{Markup.Escape(header)}[/]")
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Red)
        .Padding(1, 0, 1, 0);
}
