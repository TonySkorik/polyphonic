using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Polyphonic.TelegramBot.Infrastructure;
internal class BotUpdateHandler : IUpdateHandler
{
    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync(exception.Message);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var user = update.Message?.From;
        var text = update.Message.Text ?? string.Empty;

        if (user is null)
            return;

        // Print to console
        Console.WriteLine($"{user.FirstName} wrote {text}");

        // When we get a command, we react accordingly
        if (text.StartsWith("/"))
        {
            await HandleCommand(user.Id, text);
        }
        else if (screaming && text.Length > 0)
        {
            // To preserve the markdown, we attach entities (bold, italic..)
            await botClient.SendTextMessageAsync(user.Id, text.ToUpper(), entities: msg.Entities);
        }
        else
        {   // This is equivalent to forwarding, without the sender's name
            await botClient.CopyMessageAsync(user.Id, user.Id, msg.MessageId);
        }

    }

    async Task HandleCommand(long userId, string command)
    {
        switch (command)
        {
            case "/scream":
                screaming = true;
                break;

            case "/whisper":
                screaming = false;
                break;

            case "/menu":
                await SendMenu(userId);
                break;
        }

        await Task.CompletedTask;
    }
}
