using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.CommandHandlers.SongLink.Base;
using Polyphonic.TelegramBot.Helpers;
using Polyphonic.TelegramBot.Model;
using Songlink.Client;
using Songlink.Client.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Polyphonic.TelegramBot.CommandHandlers.SongLink;

internal class ToUniversalSongLinkBotCommandHandler(
    SongLinkClient songLinkClient,
    ILogger<ToUniversalSongLinkBotCommandHandler> logger) : SongLinkConverterBotCommandHandlerBase, IBotCommandHandler
{
    public const string COMMAND_NAME = "convert";

    public (bool CanHandleInMessage, bool CanHandleInline) CanHandle(ParsedBotCommand command) =>
        (command.CommandName == COMMAND_NAME, true);

    public async Task HandleAsync(
        ITelegramBotClient botClient,
        Message message,
        ParsedBotCommand command,
        CancellationToken cancellationToken)
    {
        var (_, sender) = message.GetSender();

        var (hasValidSongShareLink, songShareLink) =
            await TryGetSongShareLinkFromCommand(
                botClient,
                sender,
                command,
                isSendErrorMessagesToChat: false,
                cancellationToken);

        if (!hasValidSongShareLink)
        {
            return;
        }

        await botClient.SendMessage(
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
                
                await botClient.SendMessage(
                    sender.Id,
                    $"Can't get universal song share link, for {songShareLink}",
                    cancellationToken: cancellationToken);
                
                return;
            }

            await botClient.SendMessage(
                sender.Id,
                allSongLinksResponse.PageUrl,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(
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
        var (hasValidSongShareLink, songShareLink) =
            await TryGetSongShareLinkFromCommand(
                botClient,
                inlineQuery.From,
                command,
                isSendErrorMessagesToChat: false,
                cancellationToken);

        if (!hasValidSongShareLink)
        {
            return;
        }
        
        try
        {
            var allSongLinksResponse =
                await songLinkClient.GetAllSongLinksAsync(songShareLink, cancellationToken);

            if (!allSongLinksResponse.IsSuccess 
                || allSongLinksResponse.LinksByPlatform is null)
            { 
                logger.LogInformation(
                    "Failed to get universal song share link, for '{SongShareLink}'. Songlink API response is success: {ApiResponseIsSuccess}, platform dictionary is empty: {PlatformCount}",
                    inlineQuery.Query,
                    allSongLinksResponse.IsSuccess,
                    allSongLinksResponse.LinksByPlatform is null
                    );
                
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
            TryAddSpecificPlatformInlineResult(inlineQueryResults, allSongLinksResponse, SongLinkPlatform.YoutubeMusic);
            TryAddSpecificPlatformInlineResult(inlineQueryResults, allSongLinksResponse, SongLinkPlatform.AppleMusic);
            
            await botClient.AnswerInlineQuery(
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
        if (!songLinkResponse.LinksByPlatform!.TryGetValue(targetPlatform, out var specificPlatformLink))
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
