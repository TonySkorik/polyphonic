using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Polyphonic.TelegramBot.Configuration;
using Polyphonic.TelegramBot.Infrastructure;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Polyphonic.TelegramBot;

internal class PolyphonicTelegramBot : IHostedService
{
    // Store bot screaming status
    readonly bool _screaming = false;

    // Pre-assign menu text
    private const string FIRST_MENU = "<b>Menu 1</b>\n\nA beautiful menu with a shiny inline button.";
    private const string SECOND_MENU = "<b>Menu 2</b>\n\nA better menu with even more shiny inline buttons.";

    // Pre-assign button text
    private const string NEXT_BUTTON = "Next";
    private const string BACK_BUTTON = "Back";
    private const string TUTORIAL_BUTTON = "Tutorial";

    private IServiceProvider _serviceProvider;
    private readonly BotConfiguration _botConfiguration;

    // Build keyboards
    InlineKeyboardMarkup _firstMenuMarkup = new(InlineKeyboardButton.WithCallbackData(NEXT_BUTTON));
    InlineKeyboardMarkup _secondMenuMarkup = new(
        new[] {
        new[] { InlineKeyboardButton.WithCallbackData(BACK_BUTTON) },
        new[] { InlineKeyboardButton.WithUrl(TUTORIAL_BUTTON, "https://core.telegram.org/bots/tutorial") }
        }
    );

    public PolyphonicTelegramBot(
        IServiceProvider serviceProvider,
        IOptions<BotConfiguration> options)
    {
        _serviceProvider = serviceProvider;
        _botConfiguration = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return StartBotAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task StartBotAsync()
    {
        var bot = new TelegramBotClient("<YOUR_BOT_TOKEN_HERE>");

        var handler = new BotUpdateHandler();

        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool, so we use cancellation token
        bot.StartReceiving(
            handler,
            cancellationToken: cts.Token
        );

        // Tell the user the bot is online
        Console.WriteLine("Start listening for updates. Press enter to stop");
        Console.ReadLine();

        // Send cancellation request to stop the bot
        cts.Cancel();
    }

    // Each time a user interacts with the bot, this method is called
    async Task HandleUpdate(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                await HandleMessage(update.Message!);
                break;

            // A button was pressed
            case UpdateType.CallbackQuery:
                await HandleButton(update.CallbackQuery!);
                break;
        }
    }

    async Task SendMenu(long userId)
    {
        await bot.SendTextMessageAsync(
            userId,
            FIRST_MENU,
            ParseMode.Html,
            replyMarkup: _firstMenuMarkup
        );
    }

    async Task HandleButton(CallbackQuery query)
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
        await bot.AnswerCallbackQueryAsync(query.Id);

        // Replace menu text and keyboard
        await bot.EditMessageTextAsync(
            query.Message!.Chat.Id,
            query.Message.MessageId,
            text,
            ParseMode.Html,
            replyMarkup: markup
        );
    }
}
