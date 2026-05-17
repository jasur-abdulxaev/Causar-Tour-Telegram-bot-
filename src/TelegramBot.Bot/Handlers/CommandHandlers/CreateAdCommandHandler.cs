using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Bot.States;

namespace TelegramBot.Bot.Handlers.CommandHandlers;

public class CreateAdCommandHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepo;

    public CreateAdCommandHandler(ITelegramBotClient bot, IUserRepository userRepo)
    {
        _bot = bot;
        _userRepo = userRepo;
    }

    public async Task StartAsync(long chatId, long fromId, CancellationToken ct)
    {
        // Yangi e'lon yaratish holati — birinchi qadamdan boshlaymiz
        var state = new AdCreationState { Step = AdCreationStep.WaitingTitle };

        var user = await _userRepo.GetByTelegramIdAsync(fromId, ct);
        if (user != null)
        {
            user.ConversationState = state.ToJson();
            await _userRepo.UpsertAsync(user, ct);
        }

        await _bot.SendMessage(chatId,
            "✏️ <b>Yangi e'lon yaratish</b>\n\n" +
            "<b>1-qadam:</b> E'lon sarlavhasini kiriting:\n" +
            "<i>Misol: Toshkent → Antaliya 🌴</i>",
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
}
