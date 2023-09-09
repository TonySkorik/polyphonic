open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open Telegram.Bot.Polling
open System
open System.IO
open Polyphonic.TelegramBot.Infrastructure
open Polyphonic.TelegramBot
open Polyphonic.TelegramBot.Configuration
open Serilog
open Serilog.Extensions.Logging

let AddLogger<'T> (services: IServiceCollection) =
    let logger =
        LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger()

    let loggerFactory = new SerilogLoggerFactory(logger)

    let microsoftLogger = loggerFactory.CreateLogger<'T>()

    services
        .AddLogging(fun builder ->
            builder.ClearProviders().AddSerilog(logger).SetMinimumLevel(LogLevel.Trace)
            |> ignore)
        .AddSingleton(microsoftLogger)
    |> ignore

type Program() =
    static member Main(args: string[]) =
        let configuration =
            (new ConfigurationBuilder())
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()

        let builder =
            Host.CreateDefaultBuilder(args)
            |> fun builder ->
                builder.ConfigureServices(fun services ->
                    services
                        .AddSingleton(configuration)
                        .AddSingleton<IUpdateHandler, BotUpdateHandler>()
                        .AddSingleton<PolyphonicTelegramBot>()
                        .Configure<BotConfiguration>(fun options ->
                            configuration.GetSection("BotConfiguration").Bind(options))
                    |> AddLogger<Program>
                    |> ignore)

        use host = builder.Build()
        host.Run()

Program.Main(Environment.GetCommandLineArgs())
