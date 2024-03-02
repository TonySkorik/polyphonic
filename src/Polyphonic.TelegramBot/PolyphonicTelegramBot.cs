using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polyphonic.TelegramBot.Configuration;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace Polyphonic.TelegramBot;

internal class PolyphonicTelegramBot(
    IServiceProvider serviceProvider,
    IOptions<BotConfiguration> options,
    ILogger<PolyphonicTelegramBot> logger) : IHostedService
{
    private readonly BotConfiguration _botConfiguration = options.Value;
    private readonly CancellationTokenSource _killswitch = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return StartBotAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _killswitch.CancelAsync();
    }

    private Task StartBotAsync()
    {
        var handler = serviceProvider.GetRequiredService<IUpdateHandler>();
        var exceptionsParser = serviceProvider.GetRequiredService<IExceptionParser>();
        
        var bot = new TelegramBotClient(_botConfiguration.BotAccessToken){
            ExceptionsParser = exceptionsParser
        };
        
        bot.StartReceiving(
            handler,
            cancellationToken: _killswitch.Token
        );

        // Tell the user the bot is online
        logger.LogInformation("Started listening for updates");

#if DEBUG
        Console.ReadLine();
        _killswitch.Cancel();
#endif

        return Task.CompletedTask;
    }
}
