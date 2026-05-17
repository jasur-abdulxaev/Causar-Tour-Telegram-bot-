using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramBot.Domain.Entities;

namespace TelegramBot.Infrastructure.Data.Configurations;

public class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(300);
        builder.Property(a => a.Description).HasMaxLength(4000);
        builder.Property(a => a.Direction).HasMaxLength(200);
        builder.Property(a => a.TransportInfo).HasMaxLength(300);
        builder.Property(a => a.ContactInfo).HasMaxLength(500);
        builder.Property(a => a.MediaFileId).HasMaxLength(200);
        builder.Property(a => a.MediaType).HasMaxLength(20);

        builder.HasMany(a => a.Hotels)
            .WithOne(h => h.Advertisement)
            .HasForeignKey(h => h.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Leads)
            .WithOne(l => l.Advertisement)
            .HasForeignKey(l => l.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Analytics)
            .WithOne(an => an.Advertisement)
            .HasForeignKey(an => an.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.CreatedByAdminId);
    }
}

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        builder.Property(h => h.PriceUsd).HasColumnType("decimal(10,2)");
        builder.Property(h => h.ExtraInfo).HasMaxLength(500);
    }
}

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Username).HasMaxLength(100);
        builder.Property(l => l.FirstName).HasMaxLength(100);
        builder.Property(l => l.LastName).HasMaxLength(100);
        builder.HasIndex(l => new { l.AdvertisementId, l.TelegramUserId });
    }
}

public class BotUserConfiguration : IEntityTypeConfiguration<BotUser>
{
    public void Configure(EntityTypeBuilder<BotUser> builder)
    {
        builder.HasKey(u => u.TelegramId);
        builder.Property(u => u.Username).HasMaxLength(100);
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.PreferredLanguage).HasMaxLength(5).HasDefaultValue("ru");
        builder.Property(u => u.ConversationState).HasMaxLength(4000);

        // Broadcast query: WHERE IsAdmin=0 AND (LastBroadcastSentAt IS NULL OR LastBroadcastSentAt <= cutoff)
        builder.HasIndex(u => new { u.IsAdmin, u.LastBroadcastSentAt });
    }
}

public class AdAnalyticsConfiguration : IEntityTypeConfiguration<AdAnalytics>
{
    public void Configure(EntityTypeBuilder<AdAnalytics> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EventType).IsRequired().HasMaxLength(50);
        builder.HasIndex(a => new { a.AdvertisementId, a.EventType });
        builder.HasIndex(a => a.OccurredAt);
    }
}
