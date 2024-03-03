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
    private readonly TaskCompletionSource _mainThreadBlocker = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // this is a blocking call
        StartBotCore();
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _mainThreadBlocker.SetResult();
        
        await _killswitch.CancelAsync();
    }

    private void StartBotCore()
    {
        var handler = serviceProvider.GetRequiredService<IUpdateHandler>();
        var exceptionsParser = serviceProvider.GetRequiredService<IExceptionParser>();
        
        var bot = new TelegramBotClient(_botConfiguration.BotAccessToken){
            ExceptionsParser = exceptionsParser
        };
        
        // this is a non-blocking call
        bot.StartReceiving(
            handler,
            cancellationToken: _killswitch.Token
        );

        logger.LogInformation("Started listening for updates");

        // block the thread to keep the console app from closing
        _mainThreadBlocker.Task.Wait();
        
        _killswitch.Cancel();
    }
}
