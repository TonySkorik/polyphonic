using Polyphonic.TelegramBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Abstractions;

internal interface IBotCommandHandler
{
	public static bool IsCommand(string messageText) => messageText.StartsWith('/');

	public static ParsedBotCommand ParseCommand(string? commandWithArguments)
	{
		if (commandWithArguments is null or {Length: 0})
		{
			return ParsedBotCommand.Invalid("NULL_COMMAND");
		}

		if (!commandWithArguments.StartsWith('/')
			|| commandWithArguments.Length < 2
			|| commandWithArguments[1] == '/')
		{
			return ParsedBotCommand.Invalid(commandWithArguments);
		}

		if (!commandWithArguments.Contains(' '))
		{
			// means we get a command without arguments
			return
				new ParsedBotCommand(true, commandWithArguments[1..], null);
		}

		var firstSpace = commandWithArguments.IndexOf(' ');

		var command = commandWithArguments[1..firstSpace];
		
		// +2 since to account for leading / and a space before first argument
		var commandArguments = commandWithArguments[(command.Length+2)..];

		return new ParsedBotCommand(true, command, commandArguments.Trim());
	}
	
	public bool CanHandle(ParsedBotCommand command);

	public Task HandleAsync(ITelegramBotClient botClient, Message message, ParsedBotCommand command, CancellationToken cancellationToken);
}
