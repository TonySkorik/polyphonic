namespace Polyphonic.TelegramBot.Model;

internal record ParsedBotCommand(
	bool IsCommandValid, 
	string CommandName, 
	string? CommandArgumentsString)
{
	public static ParsedBotCommand Invalid(string invalidCommandText)
		=> new(IsCommandValid: false, CommandName: invalidCommandText, CommandArgumentsString: null);
}
