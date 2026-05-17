using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.Handlers.CallbackHandlers;
using TelegramBot.Bot.Handlers.CommandHandlers;
using TelegramBot.Bot.Handlers.MessageHandlers;
using TelegramBot.Bot.States;

namespace TelegramBot.Bot.Handlers;

public class UpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepo;
    private readonly StartCommandHandler _startHandler;
    private readonly CreateAdCommandHandler _createAdHandler;
    private readonly MyAdsCommandHandler _myAdsHandler;
    private readonly StatsCommandHandler _statsHandler;
    private readonly TextMessageHandler _textHandler;
    private readonly PhotoMessageHandler _photoHandler;
    private readonly InterestCallbackHandler _interestHandler;
    private readonly AdminActionCallbackHandler _adminActionHandler;
    private readonly BotSettings _settings;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        ITelegramBotClient bot,
        IUserRepository userRepo,
        StartCommandHandler startHandler,
        CreateAdCommandHandler createAdHandler,
        MyAdsCommandHandler myAdsHandler,
        StatsCommandHandler statsHandler,
        TextMessageHandler textHandler,
        PhotoMessageHandler photoHandler,
        InterestCallbackHandler interestHandler,
        AdminActionCallbackHandler adminActionHandler,
        IOptions<BotSettings> settings,
        ILogger<UpdateHandler> logger)
    {
        _bot = bot;
        _userRepo = userRepo;
        _startHandler = startHandler;
        _createAdHandler = createAdHandler;
        _myAdsHandler = myAdsHandler;
        _statsHandler = statsHandler;
        _textHandler = textHandler;
        _photoHandler = photoHandler;
        _interestHandler = interestHandler;
        _adminActionHandler = adminActionHandler;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task HandleAsync(Update update, CancellationToken ct)
    {
        try
        {
            if (update.Type == UpdateType.Message && update.Message != null)
                await HandleMessageAsync(update.Message, ct);
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                await HandleCallbackAsync(update.CallbackQuery, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing update {UpdateId}", update.Id);
        }
    }

    private async Task HandleMessageAsync(Message msg, CancellationToken ct)
    {
        var from = msg.From;
        if (from == null) return;

        // Foydalanuvchini upsert qilamiz
        var botUser = new Domain.Entities.BotUser
        {
            TelegramId = from.Id,
            Username   = from.Username,
            FirstName  = from.FirstName,
            LastName   = from.LastName,
            IsAdmin    = _settings.AdminIds.Contains(from.Id)
        };
        await _userRepo.UpsertAsync(botUser, ct);

        bool isAdmin = _settings.AdminIds.Contains(from.Id);

        // ── Buyruqlar ──────────────────────────────────────────────────────
        if (msg.Text?.StartsWith("/") == true)
        {
            var command = msg.Text.Split(' ')[0].Split('@')[0].ToLower();
            switch (command)
            {
                case "/start":
                    await _startHandler.HandleAsync(msg, isAdmin, ct);
                    return;
                case "/create_ad" when isAdmin:
                    await _createAdHandler.StartAsync(msg.Chat.Id, msg.From!.Id, ct);
                    return;
                case "/my_ads" when isAdmin:
                    await _myAdsHandler.HandleAsync(msg.Chat.Id, msg.From!.Id, 1, null, ct);
                    return;
                case "/stats" when isAdmin:
                    await _statsHandler.HandleAsync(msg.Chat.Id, msg.From!.Id, ct);
                    return;
                default:
                    if (!isAdmin)
                        await _bot.SendMessage(msg.Chat.Id,
                            "Bu buyruq mavjud emas.", cancellationToken: ct);
                    return;
            }
        }

        // ── Admin uchun holat asosida yo'naltirish ─────────────────────────
        if (isAdmin)
        {
            var user = await _userRepo.GetByTelegramIdAsync(from.Id, ct);
            var stateStr = user?.ConversationState;
            var step = AdCreationState.FromJson(stateStr)?.Step ?? AdCreationStep.None;

            // Rasm yoki video keldi
            if (msg.Photo != null || msg.Video != null)
            {
                // Agar 2-qadamda (matn so'ralganda) rasm/video caption bilan kelsa,
                // yoki 3-qadamda (rasm so'ralganda) kelsa -> PhotoHandler ishlasin
                if (step == AdCreationStep.WaitingFullText || step == AdCreationStep.WaitingMedia)
                {
                    await _photoHandler.HandleAsync(msg, ct);
                    return;
                }
            }

            // TextMessageHandler barcha textli xabarlarni ishlaydi
            if (msg.Text != null)
            {
                await _textHandler.HandleAsync(msg, ct);
                return;
            }
        }
    }

    private async Task HandleCallbackAsync(CallbackQuery cb, CancellationToken ct)
    {
        if (cb.Data == null) return;
        var from = cb.From;
        bool isAdmin = _settings.AdminIds.Contains(from.Id);

        // Kanaldan foydalanuvchi "Qiziqaman" tugmasini bosgan holat
        // (Bu URL tugma bo'lgani uchun callback kelmaydi — deep link ishlaydi)
        // Qolgan barcha callbacklar admin uchun
        if (isAdmin)
        {
            switch (cb.Data)
            {
                case "create_ad":
                    await _createAdHandler.StartAsync(cb.Message!.Chat.Id, from.Id, ct);
                    break;
                case "my_ads":
                    await _myAdsHandler.HandleAsync(cb.Message!.Chat.Id, from.Id, 1, null, ct);
                    break;
                case var s when s.StartsWith("my_ads_page_"):
                    if (int.TryParse(s.Replace("my_ads_page_", ""), out var page))
                        await _myAdsHandler.HandleAsync(cb.Message!.Chat.Id, from.Id, page, cb.Message.MessageId, ct);
                    break;
                case "stats_all":
                    await _statsHandler.HandleAsync(cb.Message!.Chat.Id, from.Id, ct);
                    break;
                default:
                    await _adminActionHandler.HandleAsync(cb, ct);
                    break;
            }
        }

        // Callback query ga javob beramiz (loading animatsiyasini o'chirish uchun)
        try { await _bot.AnswerCallbackQuery(cb.Id, cancellationToken: ct); }
        catch { /* ignore */ }
    }
}
