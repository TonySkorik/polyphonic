using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.CommandHandlers.SongLink.Base;
using Polyphonic.TelegramBot.Helpers;
using Polyphonic.TelegramBot.Model;
using Songlink.Client;
using Songlink.Client.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.CommandHandlers.SongLink;

internal class ToSpecifiedSongLinkBotCommandHandler(
	SongLinkClient songLinkClient,
	ILogger<ToSpecifiedSongLinkBotCommandHandler> logger) : SongLinkConverterBotCommandHandlerBase, IBotCommandHandler
{
	private static Dictionary<string, SongLinkPlatform> _platformsByAllowedCommands = new()
	{
		["toyandex"] = SongLinkPlatform.Yandex,
		["tospotify"] = SongLinkPlatform.Spotify,
		["toyoutube"] = SongLinkPlatform.Youtube,
		["toyoutubemusic"] = SongLinkPlatform.YoutubeMusic,
		["toapple"] = SongLinkPlatform.AppleMusic,

	};

	public (bool CanHandleInMessage, bool CanHandleInline) CanHandle(ParsedBotCommand command)
		=>
			(_platformsByAllowedCommands.ContainsKey(command.CommandName), false);

	public async Task HandleAsync(
		ITelegramBotClient botClient,
		Message message,
		ParsedBotCommand command,
		CancellationToken cancellationToken)
	{
		var (_, sender) = message.GetSender();

		var (hasValidSongShareLink, songShareLink) = await TryGetSongShareLinkFromCommand(
            botClient,
            sender,
            command,
            isSendErrorMessagesToChat: false,
            cancellationToken);

		if (!hasValidSongShareLink)
		{
			return;
		}

		SongLinkPlatform targetMusicPlatform = _platformsByAllowedCommands.GetValueOrDefault(
			command.CommandName,
			SongLinkPlatform.Unknown);

		if (targetMusicPlatform == SongLinkPlatform.Unknown)
		{
			await botClient.SendMessage(
				sender.Id,
				"Unknown target song link platform. Check command name.",
				cancellationToken: cancellationToken);

			return;
		}

		await botClient.SendMessage(
			sender.Id,
			$"Getting {targetMusicPlatform} song share link, please wait...",
			cancellationToken: cancellationToken);

		try
		{
			var allSongLinksResponse =
				await songLinkClient.GetAllSongLinksAsync(songShareLink, cancellationToken);

			if (!allSongLinksResponse.IsSuccess 
				|| allSongLinksResponse.LinksByPlatform is null or {Count: 0})
			{
				logger.LogInformation(
					"Failed to get {TargetMusicPlatform} song share link, for {SongShareLink}'",
					targetMusicPlatform,
					command.CommandArgumentsString);

				await botClient.SendMessage(
					sender.Id,
					$"Can't get {targetMusicPlatform} song share link, for {command.CommandArgumentsString}",
					cancellationToken: cancellationToken);

				return;
			}

			if (!allSongLinksResponse.LinksByPlatform.TryGetValue(targetMusicPlatform, out var targetPlatformSongLinks))
			{
				await botClient.SendMessage(
					sender.Id,
					$"No share link for platform {targetMusicPlatform} found.",
					cancellationToken: cancellationToken);

				return;
			}

			await botClient.SendMessage(
				sender.Id,
				targetPlatformSongLinks.Url,
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

	public Task HandleAsync(
		ITelegramBotClient botClient,
		InlineQuery inlineQuery,
		ParsedBotCommand command,
		CancellationToken cancellationToken)
	{
		logger.LogInformation(
			"An inline query was issued to handler {HandlerName}. Handler can't handle inline queries",
			nameof(
				ToSpecifiedSongLinkBotCommandHandler));

		return Task.CompletedTask;
	}
}
