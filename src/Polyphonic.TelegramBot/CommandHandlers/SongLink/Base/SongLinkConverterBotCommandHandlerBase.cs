using Polyphonic.TelegramBot.Helpers;
using Polyphonic.TelegramBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.CommandHandlers.SongLink.Base;

internal class SongLinkConverterBotCommandHandlerBase
{
	private static readonly Uri _invalidShongShareLink = new("https://polyphonic.com/invalid-song-link-url");
	
	protected async Task<(bool HasValidSongShareLink, Uri SongShareLink)> TryGetSongShareLinkFromCommand(
		ITelegramBotClient botClient,
		Message message,
		ParsedBotCommand command,
		CancellationToken cancellationToken)
	{
		var (_, sender) = message.GetSender();
		
		if (command.CommandArgumentsString is null or {Length: 0})
		{
			await botClient.SendTextMessageAsync(
				sender.Id,
				"Get song link command must be followed by an song share url string",
				cancellationToken: cancellationToken);

			return (false, _invalidShongShareLink);
		}

		if (!Uri.TryCreate(command.CommandArgumentsString, UriKind.Absolute, out var songShareLink))
		{
			await botClient.SendTextMessageAsync(
				sender.Id,
				$"Invalid song share url '{command.CommandArgumentsString}'",
				cancellationToken: cancellationToken);

			return (false, _invalidShongShareLink);
		}
		
		return (true, songShareLink);
	}
}
