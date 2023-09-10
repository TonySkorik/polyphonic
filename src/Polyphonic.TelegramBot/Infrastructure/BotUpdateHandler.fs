namespace Polyphonic.TelegramBot.Infrastructure

open System
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
            botClient.SendTextMessageAsync(userId, firstMenu, parseMode = ParseMode.Html, replyMarkup = firstMenuMarkup)
            |> Async.AwaitTask
            |> ignore
        }
        |> Async.StartAsTask

    let handleCommand (botClient: ITelegramBotClient, userId: int64, command: string) =
        async {
            match command with
            | "/scream" -> isScreaming <- true
            | "/whisper" -> isScreaming <- false
            | "/menu" -> do! sendMenu botClient userId |> Async.AwaitTask
            | _ -> ()
        }
        |> Async.StartAsTask

    let handleButton (botClient: ITelegramBotClient, query: CallbackQuery) =
        async {
            let mutable text = ""
            let mutable markup = new InlineKeyboardMarkup(inlineKeyboard = [||])

            match query.Data with
            | "Next" ->
                text <- secondMenu
                markup <- secondMenuMarkup
            | "Back" ->
                text <- firstMenu
                markup <- firstMenuMarkup
            | _ -> ()

            do! botClient.AnswerCallbackQueryAsync(query.Id) |> Async.AwaitTask

            botClient.EditMessageTextAsync(
                chatId = query.Message.Chat.Id,
                messageId = query.Message.MessageId,
                text = text,
                parseMode = ParseMode.Html,
                replyMarkup = markup
            )
            |> Async.AwaitTask
            |> ignore
        }
        |> Async.StartAsTask

    interface IUpdateHandler with
        member this.HandlePollingErrorAsync(botClient, e: Exception, cancellationToken) =
            logger.LogError(e.Message)
            Task.CompletedTask

        member this.HandleUpdateAsync(botClient, update, cancellationToken) =
            task {
                match update.Type with
                | UpdateType.Message ->
                    let message = update.Message
                    let user = message.From
                    let text: string = message.Text

                    let username = user.FirstName
                    Console.WriteLine($"{username} wrote {text}")

                    if text.StartsWith("/") then
                        do! handleCommand (botClient, user.Id, text) |> Async.AwaitTask
                    elif isScreaming && text.Length > 0 then
                        botClient.SendTextMessageAsync(user.Id, text.ToUpper(), entities = message.Entities)
                        |> Async.AwaitTask
                        |> ignore
                    else
                        let! _ =
                            botClient.CopyMessageAsync(user.Id, user.Id, message.MessageId)
                            |> Async.AwaitTask

                        () // same as |> ignore ???

                    ()
                | UpdateType.CallbackQuery ->
                    do!
                        handleButton (botClient, update.CallbackQuery)
                        |> Async.AwaitTask
                        |> Async.Ignore
                | _ -> ()
            }
