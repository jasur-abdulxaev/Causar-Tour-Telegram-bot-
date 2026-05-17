using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBot.Infrastructure.Data;

namespace TelegramBot.Bot.BackgroundServices;

/// <summary>
/// Haftada bir marta (yakshanba tunda) ishlaydigan texnik xizmat:
///
/// 1. Eski AdAnalytics yozuvlarini o'chiradi (retention kunidan eski).
///    Sabab: broadcast'dan keyin har bir user uchun yozuv qo'shiladi —
///    bu jadval eng tez o'sadigan jadvaldir.
///
/// 2. SQLite VACUUM — o'chirilgan yozuvlar bo'shatgan joyni fayl darajasida
///    qaytaradi. SQLite bu ishni o'zi avtomatik bajarmaydi.
///
/// 3. PRAGMA incremental_vacuum — auto_vacuum=INCREMENTAL rejimida qolgan
///    sahifalarni ham tozalaydi.
/// </summary>
public class DatabaseMaintenanceBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseMaintenanceBackgroundService> _logger;

    // Eski analytics ma'lumotlarni qancha kun saqlash (180 kun = 6 oy)
    private const int AnalyticsRetentionDays = 180;

    // Texnik xizmat qanchalik tez-tez ishlaydi (haftada 1 marta)
    private static readonly TimeSpan MaintenanceInterval = TimeSpan.FromDays(7);

    // Birinchi ishga tushishda qancha vaqt kutish
    // (boshqa servislar yuklanib bo'lishi uchun)
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(2);

    public DatabaseMaintenanceBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseMaintenanceBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DatabaseMaintenanceBackgroundService ishga tushdi.");

        // Ishga tushganda biroz kutamiz
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunMaintenanceAsync(stoppingToken);
            await Task.Delay(MaintenanceInterval, stoppingToken);
        }
    }

    private async Task RunMaintenanceAsync(CancellationToken ct)
    {
        _logger.LogInformation("DB texnik xizmati boshlanmoqda...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // ── 1. Eski AdAnalytics yozuvlarini o'chirish ────────────────
            var cutoff = DateTime.UtcNow.AddDays(-AnalyticsRetentionDays);

            var deletedCount = await db.AdAnalytics
                .Where(a => a.OccurredAt < cutoff)
                .ExecuteDeleteAsync(ct);

            if (deletedCount > 0)
                _logger.LogInformation(
                    "AdAnalytics: {Count} ta eski yozuv o'chirildi (>{Days} kunlik).",
                    deletedCount, AnalyticsRetentionDays);
            else
                _logger.LogInformation("AdAnalytics: eski yozuv topilmadi.");

            // ── 2. Nashr qilingan va 90 kundan eski e'lonlar statistikasini
            //       alohida tozalash (e'lonning o'zi saqlanadi, faqat analytics)
            var adCutoff = DateTime.UtcNow.AddDays(-90);
            var oldAdAnalytics = await db.AdAnalytics
                .Where(a => a.OccurredAt < adCutoff
                    && a.Advertisement.IsPublished
                    && a.Advertisement.CreatedAt < adCutoff)
                .ExecuteDeleteAsync(ct);

            if (oldAdAnalytics > 0)
                _logger.LogInformation(
                    "Eski e'lonlar analytics: {Count} ta qo'shimcha yozuv tozalandi.", oldAdAnalytics);

            // ── 3. SQLite VACUUM (joyni qaytarish) ───────────────────────
            // Bu operatsiya vaqt olishi mumkin — shuning uchun alohida bajaramiz
            await db.Database.ExecuteSqlRawAsync("PRAGMA incremental_vacuum;", ct);

            // To'liq VACUUM — haftada bir marta qilish yaxshi
            await db.Database.ExecuteSqlRawAsync("VACUUM;", ct);

            // ── 4. DB hajmini log'ga yozamiz ─────────────────────────────
            var pageSizeResult = await db.Database
                .SqlQueryRaw<long>("SELECT page_size FROM pragma_page_size")
                .ToListAsync(ct);
            var pageCountResult = await db.Database
                .SqlQueryRaw<long>("SELECT page_count FROM pragma_page_count")
                .ToListAsync(ct);

            if (pageSizeResult.Count > 0 && pageCountResult.Count > 0)
            {
                var dbSizeMb = (pageSizeResult[0] * pageCountResult[0]) / (1024.0 * 1024.0);
                _logger.LogInformation("DB hajmi: {SizeMb:F2} MB.", dbSizeMb);
            }

            sw.Stop();
            _logger.LogInformation(
                "DB texnik xizmati yakunlandi. Davom etdi: {Elapsed}.",
                sw.Elapsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DB texnik xizmatida xato yuz berdi.");
        }
    }
}
