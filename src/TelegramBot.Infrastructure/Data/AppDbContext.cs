using Microsoft.EntityFrameworkCore;
using TelegramBot.Domain.Entities;

namespace TelegramBot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<BotUser> BotUsers => Set<BotUser>();
    public DbSet<AdAnalytics> AdAnalytics => Set<AdAnalytics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// SQLite-specific PRAGMA optimizations.
    /// WAL mode — yaxshiroq parallel o'qish/yozish.
    /// auto_vacuum=INCREMENTAL — DB fayli o'chgan yozuvlardan keyin
    /// joyni avtomatik qaytaradi (to'liq VACUUM'siz).
    /// </summary>
    public async Task ApplySqlitePragmasAsync()
    {
        await Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
        await Database.ExecuteSqlRawAsync("PRAGMA auto_vacuum=INCREMENTAL;");
        await Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;");
        await Database.ExecuteSqlRawAsync("PRAGMA cache_size=-64000;"); // 64MB cache
        await Database.ExecuteSqlRawAsync("PRAGMA temp_store=MEMORY;");
    }
}

