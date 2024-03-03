using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polyphonic.TelegramBot.Abstractions;
using Polyphonic.TelegramBot.Configuration;
using Polyphonic.TelegramBot.Handlers;
using Polyphonic.TelegramBot.Handlers.CommandHandlers;
using Polyphonic.TelegramBot.Handlers.CommandHandlers.SongLink;
using Polyphonic.TelegramBot.Infrastructure;

using Serilog;
using Songlink.Client;
using Songlink.Client.Configuration;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace Polyphonic.TelegramBot;

public class Program
{
    public static async Task Main(params string[]? args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(
                c =>
                    c.AddEnvironmentVariables("Polyphonic_")
            )
            .ConfigureServices((ctx, services) => {
                services.Configure<BotConfiguration>(ctx.Configuration.GetSection(nameof(BotConfiguration)));

                services.AddSingleton<IExceptionParser, BotExceptionsParser>();
                services.AddSingleton<IUpdateHandler, BotUpdateHandler>();

                services.AddHostedService<PolyphonicTelegramBot>();

                services.AddMemoryCache();

                // add songlink client

                services.AddHttpClient(
                    SonglinkClientConfiguration.SonglinkHttpClientName,
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(ctx.Configuration.GetSection("BotConfiguration:SongLinkApiUrl").Value!);
                    });
                services.AddTransient<SongLinkClient>();

                // bot command handlers

                services.AddTransient<IBotCommandHandler, ConvertToUniversalSongLinkBotCommandHandler>();
                services.AddTransient<IBotCommandHandler, ConvertToSpecifiedSongLinkBotCommandHandler>();
                services.AddTransient<IBotCommandHandler, GetMainMenuBotCommandHandler>();

                // bot query handlers

                services.AddTransient<IBotInlineQueryHandler, BotInlineQueryHandler>();
            })
            .ConfigureLogging((ctx, lc) => {
                Serilog.ILogger logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(ctx.Configuration)
                    .CreateLogger();

                lc.ClearProviders().AddSerilog(logger);
            });
        
        using IHost host = builder.Build();

        await host.RunAsync();
    }
}