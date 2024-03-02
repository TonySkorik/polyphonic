using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Polyphonic.TelegramBot.CommandHandlers;

internal class GetMainMenuBotCommandHandler : IBotCommandHandler
{
	private const string BOT_MAIN_MENU_MESSAGE = "<b>Bot menu</b>\n\nFollowing is the bot main menu.";
	
	// Pre-assign button text
	private const string MAIN_MENU_BUTTON = "Menu";
	private const string GET_SONG_LINK_BUTTON = "Get song link";

	private readonly InlineKeyboardMarkup _printMenuMarkup =
		new(InlineKeyboardButton.WithCallbackData(MAIN_MENU_BUTTON));

	private readonly InlineKeyboardMarkup _botMainMenuMarkup = new(
		new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(GET_SONG_LINK_BUTTON)
			}
			// ,
			// new[]
			// {
			//     InlineKeyboardButton.WithUrl(TUTORIAL_BUTTON, "https://core.telegram.org/bots/tutorial")
			// }
		}
	);
	
	public bool CanHandle(ParsedBotCommand command) => command.CommandName == "menu";

	public async Task HandleAsync(
		ITelegramBotClient botClient,
		Message message,
		ParsedBotCommand command,
		CancellationToken cancellationToken)
	{
		await botClient.SendTextMessageAsync(
			message.From!.Id,
			BOT_MAIN_MENU_MESSAGE,
			(int) ParseMode.Html,
			replyMarkup: _botMainMenuMarkup,
			cancellationToken: cancellationToken);
	}
}
