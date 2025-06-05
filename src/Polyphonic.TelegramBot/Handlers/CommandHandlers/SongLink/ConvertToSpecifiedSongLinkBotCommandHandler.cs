using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Handlers.CommandHandlers.SongLink.Base;
using Polyphonic.TelegramBot.Helpers;
using Polyphonic.TelegramBot.Model;
using Songlink.Client;
using Songlink.Client.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Handlers.CommandHandlers.SongLink;

internal class ConvertToSpecifiedSongLinkBotCommandHandler(
	SongLinkClient songLinkClient,
	ILogger<ConvertToSpecifiedSongLinkBotCommandHandler> logger
) : SongLinkConverterBotCommandHandlerBase, IBotCommandHandler
{
	private static readonly HashSet<string> _supportedCommands =
	[
		"toyandex",
		"tospotify",
		"toyoutube"
	];

	public (bool CanHandleInMessage, bool CanHandleInline) CanHandle(ParsedBotCommand command)
		=>
			(_supportedCommands.Contains(command.CommandName), false);

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

		SongLinkPlatform targetMusicPlatform = command.CommandName switch
		{
			"toyandex" => SongLinkPlatform.Yandex,
			"tospotify" => SongLinkPlatform.Spotify,
			"toyoutube" => SongLinkPlatform.Youtube,
			_ => SongLinkPlatform.Unknown
		};

		if (targetMusicPlatform == SongLinkPlatform.Unknown)
		{
			await botClient.SendTextMessageAsync(
				sender.Id,
				"Unknown target song link platform. Check command name.",
				cancellationToken: cancellationToken);

			return;
		}

		await botClient.SendTextMessageAsync(
			sender.Id,
			$"Getting {targetMusicPlatform} song share link, please wait...",
			cancellationToken: cancellationToken);

		try
		{
			var allSongLinksResponse =
				await songLinkClient.GetAllSongLinksAsync(songShareLink, cancellationToken);

			if (allSongLinksResponse.IsSuccess || allSongLinksResponse.LinksByPlatform is {Count: 0})
			{
				logger.LogInformation(
					"Failed to get {TargetMusicPlatform} song share link, for {SongShareLink}'",
					targetMusicPlatform,
					command.CommandArgumentsString);

				await botClient.SendTextMessageAsync(
					sender.Id,
					$"Can't get {targetMusicPlatform} song share link, for {command.CommandArgumentsString}",
					cancellationToken: cancellationToken);

				return;
			}

			if (!allSongLinksResponse.LinksByPlatform.TryGetValue(targetMusicPlatform, out var targetPlatformSongLinks))
			{
				await botClient.SendTextMessageAsync(
					sender.Id,
					$"No share link for platform {targetMusicPlatform} found.",
					cancellationToken: cancellationToken);

				return;
			}

			await botClient.SendTextMessageAsync(
				sender.Id,
				targetPlatformSongLinks.Url,
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

	public Task HandleAsync(
		ITelegramBotClient botClient,
		InlineQuery inlineQuery,
		ParsedBotCommand command,
		CancellationToken cancellationToken)
	{
		logger.LogInformation(
			"An inline query was issued to handler {HandlerName}. Handler can't handle inline queries",
			nameof(
				ConvertToSpecifiedSongLinkBotCommandHandler));

		return Task.CompletedTask;
	}
}
