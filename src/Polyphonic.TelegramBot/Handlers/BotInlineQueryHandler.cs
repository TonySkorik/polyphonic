using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Handlers;

internal class BotInlineQueryHandler(
	IEnumerable<IBotCommandHandler> commandHandlers, 
	ILogger<BotInlineQueryHandler> logger) : IBotInlineQueryHandler
{
	public async Task HandleAsync(ITelegramBotClient botClient, InlineQuery query, CancellationToken cancellationToken)
	{
		var queryString = query.Query;

		bool isCommandQuery = IBotCommandHandler.IsCommand(queryString);

		if (isCommandQuery)
		{
			var parsedCommand = IBotCommandHandler.ParseCommand(queryString);

			if (!parsedCommand.IsCommandValid)
			{
				return;
			}

			foreach (var commandHandler in commandHandlers)
			{
				if (!commandHandler.CanHandle(parsedCommand).CanHandleInline)
				{
					continue;
				}

				await commandHandler.HandleAsync(botClient, query, parsedCommand, cancellationToken);

				return;
			}
		}
		
		// means this is not a command query - check if the query is Uri

		if (!Uri.TryCreate(queryString, UriKind.Absolute, out _))
		{ 
			return;
		}
		
		// means we get an uri - issue and handle "convert" command 
		var convertConmmand = new ParsedBotCommand(true, "convert", queryString);
		
		//TODO : refactor this into something more elegant
		
		foreach (var commandHandler in commandHandlers)
		{
			if (!commandHandler.CanHandle(convertConmmand).CanHandleInline)
			{
				continue;
			}

			await commandHandler.HandleAsync(botClient, query, convertConmmand, cancellationToken);

			return;
		}
	}
}
