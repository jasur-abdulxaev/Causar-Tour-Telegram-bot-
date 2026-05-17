using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Infrastructure.Data;
using TelegramBot.Infrastructure.Repositories;

namespace TelegramBot.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=tourism_bot.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<DbInitializer>();

        return services;
    }
}
