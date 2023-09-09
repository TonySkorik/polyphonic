namespace Polyphonic.TelegramBot

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open Polyphonic.TelegramBot.Configuration
open Telegram.Bot
open Telegram.Bot.Polling

type PolyphonicTelegramBot(serviceProvider: IServiceProvider, options: IOptions<BotConfiguration>) =
    let botConfiguration = options.Value

    let StartBotAsync () =
        let bot = new TelegramBotClient(botConfiguration.BotAccessToken)

        let handler = serviceProvider.GetRequiredService<IUpdateHandler>()

        use cts = new CancellationTokenSource()

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool, so we use cancellation token
        bot.StartReceiving(handler, cancellationToken = cts.Token)

        // Tell the user the bot is online
        Console.WriteLine("Start listening for updates. Press enter to stop")
        Console.ReadLine() |> ignore

        // Send cancellation request to stop the bot
        cts.Cancel()

        Task.CompletedTask

    interface IHostedService with
        member this.StartAsync(cancellationToken) = StartBotAsync()
        member this.StopAsync(cancellationToken) = Task.CompletedTask
