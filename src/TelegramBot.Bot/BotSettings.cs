namespace TelegramBot.Bot;

public class BotSettings
{
    public string BotToken { get; set; } = string.Empty;
    public long[] AdminIds { get; set; } = Array.Empty<long>();
    public string AdminUsername { get; set; } = string.Empty;  // e.g. "Madina_Centraltur"
    public string? ChannelId { get; set; }                     // e.g. "@causartour"
    public string? ChannelUsername { get; set; }               // e.g. "causartour" (without @)

    // Haftada 2 marta broadcast jadvali
    // e.g. ["Monday", "Thursday"] — UTC soat 09:00
    public string[] BroadcastDays { get; set; } = ["Monday", "Thursday"];
    public int BroadcastHourUtc { get; set; } = 9;             // 09:00 UTC = 14:00 Toshkent
    public int BroadcastMinIntervalDays { get; set; } = 3;     // Eng kamida 3 kun oraliq
}
