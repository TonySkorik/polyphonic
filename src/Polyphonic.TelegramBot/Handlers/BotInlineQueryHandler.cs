using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Handlers;

internal class BotInlineQueryHandler(IEnumerable<IBotCommandHandler> commandHandlers) : IBotInlineQueryHandler
{
	public async Task HandleAsync(ITelegramBotClient botClient, InlineQuery query, CancellationToken cancellationToken)
	{
		var queryString = query.Query;
		
		bool isCommandQuery = IBotCommandHandler.IsCommand(queryString);

		ParsedBotCommand? botCommandFromQuery;
		
		if (isCommandQuery)
		{
			botCommandFromQuery = IBotCommandHandler.ParseCommand(queryString);

			if (!botCommandFromQuery.IsCommandValid)
			{
				return;
			}
		}
		else
		{
			// means this is not a command query - check if the query is Uri
			if (!Uri.IsWellFormedUriString(queryString, UriKind.Absolute))
			{
				// not command and not URI - return
				return;
			}
			
			// not a command but URI - issue and handle "convert" command 
			botCommandFromQuery = new ParsedBotCommand(true, "convert", queryString);
		}
		
		// handle created command
		
		foreach (var commandHandler in commandHandlers)
		{
			if (!commandHandler.CanHandle(botCommandFromQuery).CanHandleInline)
			{
				continue;
			}

			await commandHandler.HandleAsync(botClient, query, botCommandFromQuery, cancellationToken);

			return;
		}
	}
}
