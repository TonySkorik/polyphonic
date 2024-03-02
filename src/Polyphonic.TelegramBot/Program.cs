using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.CommandHandlers;
using Polyphonic.TelegramBot.CommandHandlers.SongLink;
using Polyphonic.TelegramBot.Configuration;
using Polyphonic.TelegramBot.Infrastructure;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Songlink.Client;
using Songlink.Client.Configuration;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

using IMicrosoftLoggerAlias = Microsoft.Extensions.Logging.ILogger;

namespace Polyphonic.TelegramBot;

public class Program
{
    public static async Task Main(params string[]? args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables("Polyphonic_")
            .Build();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton(configuration);
        builder.Services.AddSingleton<IExceptionParser, BotExceptionsParser>();
        builder.Services.AddSingleton<IUpdateHandler, BotUpdateHandler>();
        builder.Services.AddSingleton<PolyphonicTelegramBot>();
        builder.Services.AddHttpClient(
            SonglinkClientConfiguration.SonglinkHttpClientName,
            client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("BotConfiguration:SongLinkApiUrl").Value!);
            });
        builder.Services.AddSingleton<SongLinkClient>();

        builder.Services.Configure<BotConfiguration>(configuration.GetSection(nameof(BotConfiguration)));
        builder.Services.AddHostedService<PolyphonicTelegramBot>();
        
        // bot command handlers

        builder.Services.AddSingleton<IBotCommandHandler, ConvertToUniversalSongLinkBotCommandHandler>();
        builder.Services.AddSingleton<IBotCommandHandler, ConvertToSpecifiedSongLinkBotCommandHandler>();
        builder.Services.AddSingleton<IBotCommandHandler, GetMainMenuBotCommandHandler>();

        AddLogger(builder.Services);

        using IHost host = builder.Build();

        await host.StartAsync();
    }

    private static void AddLogger(IServiceCollection services)
    {
        //LogEventLevel minimumEventLevel = IsCiEnvironment
        //    ? LogEventLevel.Information
        //    : LogEventLevel.Verbose;

        Serilog.ILogger logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Is(LogEventLevel.Verbose)
            .CreateLogger();

        IMicrosoftLoggerAlias microsoftLogger = new SerilogLoggerFactory(logger)
            .CreateLogger<Program>();

        //LogLevel minimumMsLoggerLevel = IsCiEnvironment
        //    ? LogLevel.Information
        //    : LogLevel.Trace;

        services.AddLogging(
            builder => builder.ClearProviders()
                .AddSerilog(logger)
                .SetMinimumLevel(LogLevel.Trace)
        );

        services.AddSingleton(microsoftLogger);
    }
}