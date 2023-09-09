namespace Polyphonic.TelegramBot.Infrastructure

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Telegram.Bot
open Telegram.Bot.Types
open Telegram.Bot.Types.Enums
open Telegram.Bot.Types.ReplyMarkups
open Telegram.Bot.Polling

type BotUpdateHandler(logger: ILogger<BotUpdateHandler>) =
    let mutable isScreaming = false

    let firstMenu = "<b>Menu 1</b>\n\nA beautiful menu with a shiny inline button."

    let secondMenu =
        "<b>Menu 2</b>\n\nA better menu with even more shiny inline buttons."

    // Values that are intended to be constants can be marked with the Literal attribute.
    // This attribute has the effect of causing a value to be compiled as a constant.
    [<Literal>]
    let nextButtonText = "Next"

    [<Literal>]
    let backButtonText = "Back"

    [<Literal>]
    let tutorialButtonText = "Tutorial"

    let firstMenuMarkup =
        InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(nextButtonText))

    let backButton = [| InlineKeyboardButton.WithCallbackData(backButtonText) |]

    let tutorialButton =
        [| InlineKeyboardButton.WithUrl(tutorialButtonText, "https://core.telegram.org/bots/tutorial") |]

    let secondMenuMarkup =
        InlineKeyboardMarkup(inlineKeyboard = [| backButton; tutorialButton |])

    let sendMenu (botClient: ITelegramBotClient) (userId: int64) =
        async {
            do! botClient.SendTextMessageAsync(userId, firstMenu, ParseMode.Html, replyMarkup = firstMenuMarkup)
            |> Async.AwaitTask
        }
        |> Async.StartAsTask

    let handleCommand (botClient: ITelegramBotClient, userId: int64, command: string) =
        async {
            match command with
            | "/scream" -> isScreaming <- true
            | "/whisper" -> isScreaming <- false
            | "/menu" -> do! sendMenu botClient userId
            | _ -> ()
        }
        |> Async.StartAsTask

    let handleButton (botClient: ITelegramBotClient, query: CallbackQuery) =
        async {
            let mutable text = ""
            let mutable markup = new InlineKeyboardMarkup(inlineKeyboard = [||])

            match query.Data with
            | nextButton ->
                text <- secondMenu
                markup <- secondMenuMarkup
            | backButton ->
                text <- firstMenu
                markup <- firstMenuMarkup
            | _ -> ()

            do! botClient.AnswerCallbackQueryAsync(query.Id) |> Async.AwaitTask

            do!
                botClient.EditMessageTextAsync(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    text,
                    ParseMode.Html,
                    replyMarkup = markup
                )
            |> Async.AwaitTask
        }
        |> Async.StartAsTask

    interface IUpdateHandler with
        member this.HandlePollingErrorAsync(botClient, e: Exception, cancellationToken) =
            logger.LogError(e.Message)
            Task.CompletedTask

        member this.HandleUpdateAsync(botClient, update, cancellationToken) =
            async {
                match update.Type with
                | UpdateType.Message when not (update.Message != null) ->
                    let message = update.Message
                    let user = message.From
                    let text: string = message.Text

                    if user != null then
                        let username = user.FirstName
                        Console.WriteLine($"{username} wrote {text}")

                    if text.StartsWith("/") then
                        do! handleCommand (botClient, user.Id, text) |> Async.AwaitTask
                    elif isScreaming && text.Length > 0 then
                        do!
                            botClient.SendTextMessageAsync(user.Value.Id, text.ToUpper(), entities = message.Entities)
                            |> Async.AwaitTask
                    else
                        do!
                            botClient.CopyMessageAsync(user.Value.Id, user.Value.Id, message.MessageId)
                            |> Async.AwaitTask
                | UpdateType.CallbackQuery when not (update.CallbackQuery.IsNone) ->
                    do! handleButton (botClient, update.CallbackQuery.Value) |> Async.AwaitTask
                | _ -> ()
            }
            |> Async.StartAsTask
