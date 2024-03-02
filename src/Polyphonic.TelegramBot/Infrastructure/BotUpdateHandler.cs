using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Configuration;
using Polyphonic.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Polyphonic.TelegramBot.Infrastructure;

internal class BotUpdateHandler(
    IEnumerable<IBotCommandHandler> commandHandlers,
    ILogger<BotUpdateHandler> logger) : IUpdateHandler
{
    private readonly ILogger _logger = logger;

    public Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception happened during bot API polling");

        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botCleint,
        Update update,
        CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                await HandleMessage(botCleint, update.Message!, cancellationToken);
                break;

            // A button was pressed
            case UpdateType.CallbackQuery:
                await HandleButtonClick(botCleint, update.CallbackQuery!, cancellationToken);
                break;
        }
    }

    private async Task HandleMessage(
        ITelegramBotClient botCleint,
        Message message,
        CancellationToken cancellationToken)
    {
        var (hasSender, sender) = message.GetSender();
        var (hasMessageText, messageText) = message.GetMessageText();

        if (!hasSender)
        {
            _logger.LogError("Can't handle message. User Id is null");
            
            return;
        }

        if (!hasMessageText)
        {
            _logger.LogError("Can't handle message. Message text is null");

            return;
        }
        
        _logger.LogInformation("{UserName} wrote {MessageText}", sender.FirstName, messageText);
        
        if (IBotCommandHandler.IsCommand(messageText))
        {
            // means message is bot command
            await HandleCommand(botCleint, message, cancellationToken);
        }
        else
        { 
            // this is equivalent to forwarding, without the sender's name
            
            await botCleint.CopyMessageAsync(
                sender.Id,
                sender.Id,
                message.MessageId,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleCommand(
        ITelegramBotClient botCleint,
        Message message,
        CancellationToken cancellationToken)
    {
        var parsedCommand = IBotCommandHandler.ParseCommand(message.Text);

        if (!parsedCommand.IsCommandValid)
        {
            await botCleint.SendTextMessageAsync(
                message.From!.Id,
                $"Invalid command '{parsedCommand.CommandName}'. Command format : '/<command_name> [argument_1] ... [argument_n]'",
                cancellationToken: cancellationToken);
        }

        foreach (var commandHandler in commandHandlers)
        {
            if (!commandHandler.CanHandle(parsedCommand))
            {
                continue;
            }

            await commandHandler.HandleAsync(botCleint, message, parsedCommand, cancellationToken);
            
            return;
        }
    }

    private async Task HandleButtonClick(
        ITelegramBotClient botCleint,
        CallbackQuery query,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        string text = string.Empty;
        //InlineKeyboardMarkup markup = new(Array.Empty<InlineKeyboardButton>());

        // if (query.Data == GET_SONG_LINK_BUTTON)
        // {
        //     text = GETTING_SONG_LINK_MESSAGE;
        //     //markup = _printMenuMarkup;
        // }

        // Close the query to end the client-side loading animation
        await botCleint.AnswerCallbackQueryAsync(query.Id, cancellationToken: cancellationToken);

        // Replace menu text and keyboard
        await botCleint.EditMessageTextAsync(
            query.Message!.Chat.Id,
            query.Message.MessageId,
            text,
            ParseMode.Html,
            //replyMarkup: markup,
            cancellationToken: cancellationToken);
    }
}
