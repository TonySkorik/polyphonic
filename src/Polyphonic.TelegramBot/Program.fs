open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open  Microsoft.Extensions.Logging

//open  Polyphonic.TelegramBot.Configuration;
//open  Polyphonic.TelegramBot.Infrastructure;
open Serilog
open Serilog.Events
open Serilog.Extensions.Logging

open Telegram.Bot.Polling
open System
open System.IO
open Polyphonic.TelegramBot.Infrastructure

type Program() =
    static member Main(args : string[]) =
        let configuration =
            let builder = new ConfigurationBuilder()
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build()

        let builder =
            Host.CreateDefaultBuilder(args)
            |> fun builder ->
                builder.ConfigureServices(
                    fun services ->
                        services.AddSingleton(configuration)
                                .AddSingleton<IUpdateHandler, BotUpdateHandler>()
                                .AddSingleton<PolyphonicTelegramBot>()
                                .Configure<BotConfiguration>(fun options -> configuration.GetSection("BotConfiguration").Bind(options))
                                |> AddLogger
                )

        use host = builder.Build()
        host.Run()

Program.Main(Environment.GetCommandLineArgs())

let addLogger (services: IServiceCollection) =
    let logger = LoggerConfiguration()
                    .WriteTo.Console()
                    .MinimumLevel.Verbose()
                    .CreateLogger()

    let serilogLoggerFactory = LoggerFactoryExtensions.AddSerilog(logger)

    let microsoftLogger =
        serilogLoggerFactory.CreateLogger<Program>()

    services.AddLogging(
        Action<ILoggingBuilder>(fun builder ->
            builder.ClearProviders()
                   .AddSerilog(logger)
                   .SetMinimumLevel(LogLevel.Trace)
        )
    )

    services.AddSingleton(microsoftLogger)