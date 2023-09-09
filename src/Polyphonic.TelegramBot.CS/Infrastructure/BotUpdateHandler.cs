using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Polyphonic.TelegramBot.Infrastructure;
internal class BotUpdateHandler : IUpdateHandler
{
    // Store bot screaming status
    private bool _screaming = false;

    // Pre-assign menu text
    private const string FIRST_MENU = "<b>Menu 1</b>\n\nA beautiful menu with a shiny inline button.";
    private const string SECOND_MENU = "<b>Menu 2</b>\n\nA better menu with even more shiny inline buttons.";

    // Pre-assign button text
    private const string NEXT_BUTTON = "Next";
    private const string BACK_BUTTON = "Back";
    private const string TUTORIAL_BUTTON = "Tutorial";

    // Build keyboards
    private InlineKeyboardMarkup _firstMenuMarkup = new(InlineKeyboardButton.WithCallbackData(NEXT_BUTTON));
    private InlineKeyboardMarkup _secondMenuMarkup = new(
        new[] {
            new[] {
                InlineKeyboardButton.WithCallbackData(BACK_BUTTON)
            },
            new[] {
                InlineKeyboardButton.WithUrl(TUTORIAL_BUTTON, "https://core.telegram.org/bots/tutorial")
            }
        }
    );

    private ILogger _logger;

    public BotUpdateHandler(ILogger<BotUpdateHandler> logger)
    {
        _logger = logger;
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception.Message);

        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botCleint, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;

        var user = message?.From;
        var text = message?.Text ?? string.Empty;

        if (user is null)
        {
            return;
        }

        // Print to console
        Console.WriteLine($"{user.FirstName} wrote {text}");

        // When we get a command, we react accordingly
        if (text.StartsWith("/"))
        {
            await HandleCommand(botCleint, user.Id, text);
        }
        else if (_screaming && text.Length > 0)
        {
            // To preserve the markdown, we attach entities (bold, italic..)
            await botCleint.SendTextMessageAsync(user.Id, text.ToUpper(), entities: message.Entities);
        }
        else
        {   // This is equivalent to forwarding, without the sender's name
            await botCleint.CopyMessageAsync(user.Id, user.Id, message.MessageId);
        }

    }

    private async Task HandleCommand(ITelegramBotClient botCleint, long userId, string command)
    {
        switch (command)
        {
            case "/scream":
                _screaming = true;
                break;

            case "/whisper":
                _screaming = false;
                break;

            case "/menu":
                await SendMenu(botCleint, userId);
                break;
        }

        await Task.CompletedTask;
    }

    // Each time a user interacts with the bot, this method is called
    private async Task HandleUpdate(ITelegramBotClient botCleint, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                //await HandleMessage(bot, update.Message!);
                break;

            // A button was pressed
            case UpdateType.CallbackQuery:
                await HandleButton(botCleint, update.CallbackQuery!);
                break;
        }
    }

    private async Task SendMenu(ITelegramBotClient botCleint, long userId)
    {
        await botCleint.SendTextMessageAsync(
            userId,
            FIRST_MENU,
            (int)ParseMode.Html,
            replyMarkup: _firstMenuMarkup
        );
    }

    private async Task HandleButton(ITelegramBotClient botCleint, CallbackQuery query)
    {
        string text = string.Empty;
        InlineKeyboardMarkup markup = new(Array.Empty<InlineKeyboardButton>());

        if (query.Data == NEXT_BUTTON)
        {
            text = SECOND_MENU;
            markup = _secondMenuMarkup;
        }
        else if (query.Data == BACK_BUTTON)
        {
            text = FIRST_MENU;
            markup = _firstMenuMarkup;
        }

        // Close the query to end the client-side loading animation
        await botCleint.AnswerCallbackQueryAsync(query.Id);

        // Replace menu text and keyboard
        await botCleint.EditMessageTextAsync(
            query.Message!.Chat.Id,
            query.Message.MessageId,
            text,
            ParseMode.Html,
            replyMarkup: markup
        );
    }
}
