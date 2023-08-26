using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Polyphonic.TelegramBot.Configuration;

using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Polyphonic.TelegramBot;

internal class PolyphonicTelegramBot : IHostedService
{
    private IServiceProvider _serviceProvider;
    private readonly BotConfiguration _botConfiguration;

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

    private Task StartBotAsync()
    {
        var bot = new TelegramBotClient("<YOUR_BOT_TOKEN_HERE>");

        var handler = _serviceProvider.GetRequiredService<IUpdateHandler>();

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

        return Task.CompletedTask;
    }
}
