using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Helpers;

internal static class MessageHelper
{
	public static (bool HasSender, User Sender) GetSender(this Message message)
	{
		if (message.From is null)
		{
			return (false, new User(){Id = -1});
		}

		return (true, message.From);
	}
	
	public static (bool HasMessageText, string MessageText) GetMessageText(this Message message)
	{
		if (message.Text is {Length: > 0} messageText)
		{
			return (true, messageText);
		}

		return (false, "");
	}
}
