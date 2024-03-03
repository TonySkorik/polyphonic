using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Polyphonic.TelegramBot.Infrastructure;

internal class BotUpdateHandler(
    IServiceProvider serviceProvider,
    ILogger<BotUpdateHandler> logger) : IUpdateHandler
{
    public Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception happened during bot API polling");

        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                await HandleMessage(botClient, update.Message!, cancellationToken);
                break;

            // A button was pressed
            case UpdateType.CallbackQuery:
                await HandleButtonClick(botClient, update.CallbackQuery!, cancellationToken);
                break;
            
            // An inline query was received
            case UpdateType.InlineQuery:
            { 
                // braces to make using declaration work
                
                using var serviceScope = serviceProvider.CreateScope();
                var inlineQueryHandler = serviceScope.ServiceProvider.GetRequiredService<IBotInlineQueryHandler>();
                
                await inlineQueryHandler.HandleAsync(botClient, update.InlineQuery!, cancellationToken);
                break;
            }
        }
    }

    private async Task HandleMessage(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken)
    {
        var (hasSender, sender) = message.GetSender();
        var (hasMessageText, messageText) = message.GetMessageText();

        if (!hasSender)
        {
            logger.LogError("Can't handle message. User Id is null");
            
            return;
        }

        if (!hasMessageText)
        {
            logger.LogError("Can't handle message. Message text is null");

            return;
        }
        
        logger.LogInformation("{UserName} wrote {MessageText}", sender.FirstName, messageText);
        
        if (IBotCommandHandler.IsCommand(messageText))
        {
            // means message is bot command
            await HandleCommand(botClient, message, cancellationToken);
        }
        else
        {
            // this is equivalent to forwarding, without the sender's name
            
            await botClient.CopyMessageAsync(
                sender.Id,
                sender.Id,
                message.MessageId,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleCommand(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken)
    {
        var parsedCommand = IBotCommandHandler.ParseCommand(message.Text);

        if (!parsedCommand.IsCommandValid)
        {
            await botClient.SendTextMessageAsync(
                message.From!.Id,
                $"Invalid command '{parsedCommand.CommandName}'. Command format : '/<command_name> [argument_1] ... [argument_n]'",
                cancellationToken: cancellationToken);
        }

        using var serviceScope = serviceProvider.CreateScope();

        var commandHandlers = 
            serviceScope.ServiceProvider.GetServices<IBotCommandHandler>();

        foreach (var commandHandler in commandHandlers)
        {
            if (!commandHandler.CanHandle(parsedCommand).CanHandleInMessage)
            {
                continue;
            }

            await commandHandler.HandleAsync(botClient, message, parsedCommand, cancellationToken);
            
            return;
        }
    }

    private async Task HandleButtonClick(
        ITelegramBotClient botClient,
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
        await botClient.AnswerCallbackQueryAsync(query.Id, cancellationToken: cancellationToken);

        // Replace menu text and keyboard
        await botClient.EditMessageTextAsync(
            query.Message!.Chat.Id,
            query.Message.MessageId,
            text,
            ParseMode.Html,
            //replyMarkup: markup,
            cancellationToken: cancellationToken);
    }
}
