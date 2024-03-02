using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.CommandHandlers.SongLink.Base;
using Polyphonic.TelegramBot.Helpers;
using Polyphonic.TelegramBot.Model;
using Songlink.Client;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.CommandHandlers.SongLink;

internal class ConvertToUniversalSongLinkBotCommandHandler(SongLinkClient songLinkClient) : 
    SongLinkConverterBotCommandHandlerBase, IBotCommandHandler
{
    public bool CanHandle(ParsedBotCommand command) => command.CommandName == "convert";

    public async Task HandleAsync(
        ITelegramBotClient botClient,
        Message message,
        ParsedBotCommand command,
        CancellationToken cancellationToken)
    {
        var (_, sender) = message.GetSender();

        var (hasValidShongShareLink, songShareLink) =
            await TryGetSongShareLinkFromCommand(botClient, message, command, cancellationToken);

        if (!hasValidShongShareLink)
        { 
            return;
        }
        
        await botClient.SendTextMessageAsync(
            sender.Id,
            "Getting universal song share link, please wait...",
            cancellationToken: cancellationToken);

        try
        {
            var allSongLinksResponse =
                await songLinkClient.GetAllSongLinksAsync(songShareLink, cancellationToken);

            await botClient.SendTextMessageAsync(
                sender.Id,
                allSongLinksResponse.PageUrl,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(
                sender.Id,
                $"Error getting universal song link : {ex.Message}",
                cancellationToken: cancellationToken);
        }
    }
}
