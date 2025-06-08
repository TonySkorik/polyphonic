using Polyphonic.TelegramBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Handlers.CommandHandlers.SongLink.Base;

internal class SongLinkConverterBotCommandHandlerBase
{
	private static readonly Uri _invalidSongShareLink = new("https://polyphonic.com/invalid-song-link-url");
	
	protected async Task<(bool HasValidSongShareLink, Uri SongShareLink)> TryGetSongShareLinkFromCommand(
		ITelegramBotClient botClient,
		User user,
		ParsedBotCommand command,
		bool isSendErrorMessagesToChat,
		CancellationToken cancellationToken)
	{
		if (isSendErrorMessagesToChat && command.CommandArgumentsString is null or {Length: 0})
		{
			await botClient.SendMessage(
				user.Id,
				"Get song link command must be followed by an song share url string",
				cancellationToken: cancellationToken);

			return (false, _invalidSongShareLink);
		}

		if (!Uri.TryCreate(command.CommandArgumentsString, UriKind.Absolute, out var songShareLink))
		{
			if (isSendErrorMessagesToChat)
			{
				await botClient.SendMessage(
					user.Id,
					$"Invalid song share url '{command.CommandArgumentsString}'",
					cancellationToken: cancellationToken);
			}

			return (false, _invalidSongShareLink);
		}
		
		return (true, songShareLink);
	}
}
