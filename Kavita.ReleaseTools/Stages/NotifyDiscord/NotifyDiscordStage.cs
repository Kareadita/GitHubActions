using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Kavita.ReleaseTools.Models;
using Microsoft.Extensions.Logging;
using ExecutionContext = Kavita.ReleaseTools.Models.ExecutionContext;

namespace Kavita.ReleaseTools.Stages.NotifyDiscord;

public class NotifyDiscordStage(ILogger<NotifyDiscordStage> logger) : ConfiguredStage<NotifyDiscordConfiguration>(logger)
{

    private const int TruncationBudget = 1870;

    public override string Name => nameof(NotifyDiscordStage);
    protected override NotifyDiscordConfiguration? GetConfiguration(ReleaseConfiguration configuration)
    {
        return configuration.NotifyDiscord;
    }

    protected override IReadOnlyList<ValidationIssue> Validate(ValidationContext ctx, NotifyDiscordConfiguration config)
    {
        return [];
    }

    protected override async Task ExecuteAsync(ExecutionContext ctx, NotifyDiscordConfiguration config, CancellationToken ct)
    {
        var version = ctx.ReleaseVersion?.ToString() ?? "Unknown";

        var msg = config.Message.Replace("{Version}", version);

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"{version} - {config.PrTitle}")
            .WithColor(Color.Green)
            .WithDescription(BuildDescription(config));

        if (ctx.IsTest)
        {
            return;
        }

        using var client = new DiscordWebhookClient(config.WebhookUrl);

        await client.SendMessageAsync(username: config.Username, avatarUrl: config.Icon, embeds: [embedBuilder.Build()], text: msg);

        logger.LogInformation("Notified discord for version {Version} - {Message}", version, msg);
    }

    private static string BuildDescription(NotifyDiscordConfiguration configuration)
    {
        var body = configuration.PrDescription;

        if (string.IsNullOrEmpty(body))
            return string.Empty;

        if (body.Length <= TruncationBudget)
            return body;

        var truncated = body[..TruncationBudget];
        return $"{truncated}\n...and much more.\n\nRead full changelog: {PrLink(configuration)}";
    }

    private static string PrLink(NotifyDiscordConfiguration config) => $"https://github.com/{config.Repository}/pull/{config.PrNumber}";
}
