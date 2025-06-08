using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polyphonic.TelegramBot.Configuration;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace Polyphonic.TelegramBot;

internal class PolyphonicTelegramBotBackend : IHostedService
{
    private readonly CancellationTokenSource _killswitch = new();
    private readonly TelegramBotClient _bot;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PolyphonicTelegramBotBackend> _logger;

    public PolyphonicTelegramBotBackend(
        IServiceProvider serviceProvider,
        IExceptionParser botExceptionParser,
        IOptions<BotConfiguration> options,
        ILogger<PolyphonicTelegramBotBackend> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var botConfiguration = options.Value;

        _bot = new TelegramBotClient(botConfiguration.BotAccessToken)
        {
            ExceptionsParser = botExceptionParser
        };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // this is a blocking call
        StartBotCore();
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Stopping hosted service '{nameof(PolyphonicTelegramBotBackend)}'");

        await _killswitch.CancelAsync();
    }

    private void StartBotCore()
    {
        var handler = _serviceProvider.GetRequiredService<IUpdateHandler>();

        // TODO: filter out only logic-related updates
        ReceiverOptions receiverOptions = new()
        {
            // receive all update types except ChatMember related updates
            // AllowedUpdates = Array.Empty<UpdateType>() 
        };
        
        // this is a non-blocking call
        _bot.StartReceiving(
            handler,
            cancellationToken: _killswitch.Token
            // ,receiverOptions: receiverOptions
        );
        
        _logger.LogInformation("Started listening for updates");
    }
}
