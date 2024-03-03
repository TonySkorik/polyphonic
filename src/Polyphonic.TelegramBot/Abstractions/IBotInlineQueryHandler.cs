using Telegram.Bot;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Abstractions;

internal interface IBotInlineQueryHandler
{
	public Task HandleAsync(
		ITelegramBotClient botClient,
		InlineQuery query,
		CancellationToken cancellationToken);
}
