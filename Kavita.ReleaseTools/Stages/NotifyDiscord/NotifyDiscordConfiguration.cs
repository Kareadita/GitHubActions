using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kavita.ReleaseTools.Api;
using Kavita.ReleaseTools.Configuration;
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
    [Required(ErrorMessage = "DISCORD_WEBHOOK must be set")]
    public string? WebhookUrl { get; init; }

    /// <summary>
    /// Message
    /// </summary>
    [Required(ErrorMessage = "Message is required")]
    public string Message { get; init; } = "A new version is available! {Version}";

    /// <summary>
    /// Username
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; init; }

    /// <summary>
    /// Icon
    /// </summary>
    [Url]
    public required string Icon { get; init; } =
        "https://cdn.discordapp.com/avatars/851865280727613501/ac0f4d9a4b52148789a963ccd08d5219.webp?size=80"; // GitHub icon

    [Required(ErrorMessage = "GITHUB_REPOSITORY must be set")]
    [FromEnvironment("GITHUB_REPOSITORY")]
    public string Repository { get; init; } = string.Empty;

    [Required(ErrorMessage = "PR_NUMBER must be set")]
    [FromEnvironment("PR_NUMBER")]
    public string PrNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "PR_TITLE must be set")]
    [FromEnvironment("PR_TITLE")]
    public string PrTitle { get; init; } = string.Empty;

    [Required(ErrorMessage = "PR_DESCRIPTION must be set")]
    [FromEnvironment("PR_DESCRIPTION")]
    public string PrDescription { get; init; } = string.Empty;

}
