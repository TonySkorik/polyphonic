using Telegram.Bot.Exceptions;

namespace Polyphonic.TelegramBot.Infrastructure;

internal class BotExceptionsParser : IExceptionParser 
{
	public ApiRequestException Parse(ApiResponse apiResponse)
	{
		if (apiResponse.ErrorCode == 404)
		{
			throw new ApiRequestException(
				$"Can't connect to bot API. Message : {apiResponse.Description}. Check bot access token",
				apiResponse.ErrorCode);
		}

		throw new ApiRequestException(
			$"Bot API response does not indicate success. Message : {apiResponse.Description}",
			apiResponse.ErrorCode,
			apiResponse.Parameters);
	}
}
