using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Stages.NotifyDiscord;

public class NotifyDiscordConfiguration: IStageConfiguration
{
    /// <inheritdoc/>
    public bool Disabled { get; init; }

    /// <inheritdoc/>
    public List<ReleaseType> ReleaseTypes { get; init; } = [];

    /// <summary>
    /// Webhook the release notification is posted to. Comes from the <c>DISCORD_WEBHOOK</c>
    /// environment variable, so the webhook doesn't have to be committed.
    /// </summary>
    [FromEnvironment("DISCORD_WEBHOOK")]
    [Required(ErrorMessage = "WebhookUrl is required (DISCORD_WEBHOOK)")]
    public string? WebhookUrl { get; init; }

    [Required(ErrorMessage = "Message is required")]
    public string Message { get; init; } = "A new version is available! {Version}";

    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; init; }

    [Url]
    public required string Icon { get; init; } =
        "https://cdn.discordapp.com/avatars/851865280727613501/ac0f4d9a4b52148789a963ccd08d5219.webp?size=80"; // GitHub icon

}
