using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Bot.Services;

namespace TelegramBot.Tests.Services;

public class MessageFormatterServiceTests
{
    [Fact]
    public void FormatAd_Should_EncodeHotelName_And_PreservePriceMarkup()
    {
        var ad = new AdResponse
        {
            Title = "Summer tour",
            Hotels = new List<HotelResponse>
            {
                new() { Name = "Spa <Resort> & Suites", PriceUsd = 120 }
            }
        };

        var text = MessageFormatterService.FormatAd(ad);

        Assert.Contains("• Spa &lt;Resort&gt; &amp; Suites — <b>120$</b>", text);
        Assert.DoesNotContain("Spa <Resort>", text);
    }

    [Fact]
    public void FormatAd_Should_EncodeHotelExtraInfo()
    {
        var ad = new AdResponse
        {
            Hotels = new List<HotelResponse>
            {
                new() { Name = "Resort", PriceUsd = 120, ExtraInfo = "<b>Breakfast</b> & dinner" }
            }
        };

        var text = MessageFormatterService.FormatAd(ad);

        Assert.Contains("(<i>&lt;b&gt;Breakfast&lt;/b&gt; &amp; dinner</i>)", text);
    }
}
