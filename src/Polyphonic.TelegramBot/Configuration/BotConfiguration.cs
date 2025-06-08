namespace Polyphonic.TelegramBot.Configuration;

internal class BotConfiguration
{
    public required string BotAccessToken { get; init; }

    public required string SongLinkApiUrl { get; init; }

    public string? SongLinkApiKey { get; init; }
}
