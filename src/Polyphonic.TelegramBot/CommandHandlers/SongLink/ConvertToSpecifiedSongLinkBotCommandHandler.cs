using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.CommandHandlers.SongLink.Base;
using Polyphonic.TelegramBot.Helpers;
using Polyphonic.TelegramBot.Model;
using Songlink.Client;
using Songlink.Client.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.CommandHandlers.SongLink;

internal class ConvertToSpecifiedSongLinkBotCommandHandler(SongLinkClient songLinkClient) : 
	SongLinkConverterBotCommandHandlerBase, IBotCommandHandler
{
	private readonly HashSet<string> _supportedCommands =
	[
		"toyandex",
		"tospotify",
		"toyoutube"
	];

	public bool CanHandle(ParsedBotCommand command) => _supportedCommands.Contains(command.CommandName); 

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
}
