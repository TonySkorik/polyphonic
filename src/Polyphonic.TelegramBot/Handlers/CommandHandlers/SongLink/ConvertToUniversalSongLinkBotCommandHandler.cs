using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Handlers.CommandHandlers.SongLink.Base;
using Polyphonic.TelegramBot.Helpers;
using Polyphonic.TelegramBot.Model;
using Songlink.Client;
using Songlink.Client.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Polyphonic.TelegramBot.Handlers.CommandHandlers.SongLink;

internal class ConvertToUniversalSongLinkBotCommandHandler(
    SongLinkClient songLinkClient,
    ILogger<ConvertToUniversalSongLinkBotCommandHandler> logger
) : SongLinkConverterBotCommandHandlerBase, IBotCommandHandler
{
    public (bool CanHandleInMessage, bool CanHandleInline) CanHandle(ParsedBotCommand command) =>
        (command.CommandName == "convert", true);

    public async Task HandleAsync(
        ITelegramBotClient botClient,
        Message message,
        ParsedBotCommand command,
        CancellationToken cancellationToken)
    {
        var (_, sender) = message.GetSender();

        var (hasValidShongShareLink, songShareLink) =
            await TryGetSongShareLinkFromCommand(
                botClient,
                sender,
                command,
                isSendErrorMessagesToChat: false,
                cancellationToken);

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

            if (allSongLinksResponse.IsSuccess)
            {
                logger.LogInformation(
                    "Failed to get universal song share link, for '{SongShareLink}'",
                    command.CommandArgumentsString);
                
                await botClient.SendTextMessageAsync(
                    sender.Id,
                    $"Can't get universal song share link, for {songShareLink}",
                    cancellationToken: cancellationToken);
                
                return;
            }

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

    public async Task HandleAsync(
        ITelegramBotClient botClient,
        InlineQuery inlineQuery,
        ParsedBotCommand command,
        CancellationToken cancellationToken)
    {
        var (hasValidShongShareLink, songShareLink) =
            await TryGetSongShareLinkFromCommand(
                botClient,
                inlineQuery.From,
                command,
                isSendErrorMessagesToChat: false,
                cancellationToken);

        if (!hasValidShongShareLink)
        {
            return;
        }
        
        try
        {
            var allSongLinksResponse =
                await songLinkClient.GetAllSongLinksAsync(songShareLink, cancellationToken);

            if (!allSongLinksResponse.IsSuccess || allSongLinksResponse.LinksByPlatform is {Count: 0})
            { 
                return;
            }

            List<InlineQueryResult> inlineQueryResults = new(4)
            {
                new InlineQueryResultArticle(
                    Guid.NewGuid().ToString(),
                    "🎶 Paste universal song link 🎶",
                    new InputTextMessageContent(allSongLinksResponse.PageUrl))
            };
            
            TryAddSpecificPlatformInlineResult(inlineQueryResults, allSongLinksResponse, SongLinkPlatform.Yandex);
            TryAddSpecificPlatformInlineResult(inlineQueryResults, allSongLinksResponse, SongLinkPlatform.Spotify);
            TryAddSpecificPlatformInlineResult(inlineQueryResults, allSongLinksResponse, SongLinkPlatform.Youtube);
            
            await botClient.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results: inlineQueryResults,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Exception happened during inline query {InlineQueryString} handling",
                inlineQuery.Query);
        }
    }

    private void TryAddSpecificPlatformInlineResult(
        List<InlineQueryResult> inlineQueryResults,
        SongLinkResponse songLinkResponse,
        SongLinkPlatform targetPlatform)
    {
        if (!songLinkResponse.LinksByPlatform.TryGetValue(targetPlatform, out var specificPlatformLink))
        { 
            return;
        }

        inlineQueryResults.Add(
            new InlineQueryResultArticle(
                Guid.NewGuid().ToString(),
                $"➕ Paste {targetPlatform} song link ➕",
                new InputTextMessageContent(specificPlatformLink.Url)));
    }
}
