using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TelegramBot.Infrastructure.Data;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(AppDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
            await _context.ApplySqlitePragmasAsync();
            _logger.LogInformation("Database migration and PRAGMA configuration completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
