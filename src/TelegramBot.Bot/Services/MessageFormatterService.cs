using System.Linq;
using System.Text;
using TelegramBot.Application.DTOs.Responses;

namespace TelegramBot.Bot.Services;

/// <summary>
/// Formats Advertisement data into a beautiful Telegram HTML message.
/// </summary>
public static class MessageFormatterService
{
    public static string FormatAd(AdResponse ad)
    {
        if (!string.IsNullOrWhiteSpace(ad.CustomText))
        {
            // For custom text, we send it as is (assuming it might have HTML or just plain text)
            // But if we want to be safe, we could encode it. Let's assume plain text and encode.
            return System.Net.WebUtility.HtmlEncode(ad.CustomText);
        }

        var sb = new StringBuilder();

        sb.AppendLine($"🏝 <b>{System.Net.WebUtility.HtmlEncode(ad.Title)}</b>");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(ad.Description))
        {
            sb.AppendLine(System.Net.WebUtility.HtmlEncode(ad.Description));
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(ad.Direction))
            sb.AppendLine($"✈️ <b>{System.Net.WebUtility.HtmlEncode(ad.Direction)}</b> — {System.Net.WebUtility.HtmlEncode(ad.TransportInfo)}");

        if (ad.DepartureDate.HasValue)
            sb.AppendLine($"📅 Sana: <b>{ad.DepartureDate.Value:dd.MM.yyyy}</b>");

        if (ad.NightsCount.HasValue)
            sb.AppendLine($"🌙 Kunlar: <b>{ad.NightsCount} kecha</b>");

        sb.AppendLine();

        if (ad.Hotels != null && ad.Hotels.Any())
        {
            sb.AppendLine("🏨 <b>Mehmonxonalar va narxlar:</b>");
            foreach (var h in ad.Hotels)
            {
                var hotelLine = $"• {h.Name} — <b>{h.PriceUsd}$</b>";
                if (!string.IsNullOrEmpty(h.ExtraInfo)) hotelLine += $" (<i>{System.Net.WebUtility.HtmlEncode(h.ExtraInfo)}</i>)";
                sb.AppendLine(hotelLine);
            }
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(ad.ContactInfo))
        {
            sb.AppendLine("📞 <b>Aloqa:</b>");
            sb.AppendLine(System.Net.WebUtility.HtmlEncode(ad.ContactInfo));
        }

        return sb.ToString();
    }

    public static string FormatAdminStats(AnalyticsResponse stats)
    {
        return $"""
            <b>📊 {System.Net.WebUtility.HtmlEncode(stats.AdTitle)}</b>

            👁 Ko'rishlar: <b>{stats.TotalViews}</b>
            👆 Bosilishlar: <b>{stats.TotalClicks}</b>  (CTR: {stats.ClickThroughRate}%)
            💬 Xabar yuborildi: <b>{stats.TotalMessagesSent}</b>  (Conv: {stats.ConversionRate}%)
            """;
    }
}
