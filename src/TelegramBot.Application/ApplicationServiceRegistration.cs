using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Application.Mappings;
using TelegramBot.Application.Services;

namespace TelegramBot.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddScoped<IAdvertisementService, AdvertisementService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }
}
