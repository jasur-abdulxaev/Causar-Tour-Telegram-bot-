using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Bot.Keyboards;
using TelegramBot.Bot.States;

namespace TelegramBot.Bot.Handlers.MessageHandlers;

/// <summary>
/// Rasm yoki video xabarlarini qayta ishlaydi.
/// WaitingMedia holatida rasm/video qabul qilinadi va keyingi qadamga o'tiladi.
/// </summary>
public class PhotoMessageHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepo;

    public PhotoMessageHandler(ITelegramBotClient bot, IUserRepository userRepo)
    {
        _bot = bot;
        _userRepo = userRepo;
    }

    public async Task HandleAsync(Message msg, CancellationToken ct)
    {
        var user = await _userRepo.GetByTelegramIdAsync(msg.From!.Id, ct);
        if (user == null || string.IsNullOrEmpty(user.ConversationState)) return;

        var state = AdCreationState.FromJson(user.ConversationState);
        if (state == null) return;
        
        if (state.Step != AdCreationStep.WaitingMedia && state.Step != AdCreationStep.WaitingFullText) 
            return;

        // Rasm yoki videoni saqlash
        if (msg.Photo != null && msg.Photo.Length > 0)
        {
            state.MediaFileId = msg.Photo.Last().FileId; // Eng katta o'lchamdagi rasm
            state.MediaType = "photo";
        }
        else if (msg.Video != null)
        {
            state.MediaFileId = msg.Video.FileId;
            state.MediaType = "video";
        }
        else
        {
            return; // Boshqa turdagi fayl — e'tiborsiz qoldiramiz
        }

        // Caption bo'lsa — FullText sifatida saqlaymiz (agar oldingi matn yo'q bo'lsa)
        if (!string.IsNullOrWhiteSpace(msg.Caption) && string.IsNullOrWhiteSpace(state.FullText))
        {
            state.FullText = msg.Caption;
        }

        state.Step = AdCreationStep.WaitingCaptionPosition;
        user.ConversationState = state.ToJson();
        await _userRepo.UpsertAsync(user, ct);

        await _bot.SendMessage(msg.Chat.Id,
            "✅ Rasm/video saqlandi!\n\n" +
            "<b>Matn rasmning qayerida joylashsin?</b>",
            parseMode: ParseMode.Html,
            replyMarkup: InlineKeyboards.CaptionPositionKeyboard(),
            cancellationToken: ct);
    }
}
