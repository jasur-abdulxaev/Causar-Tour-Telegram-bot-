namespace TelegramBot.Domain.Enums;

public enum LeadStatus
{
    Clicked = 0,      // user clicked the "Qiziqaman" button
    MessageSent = 1,  // user sent a message to admin
    Converted = 2,    // admin marked as converted
    Closed = 3
}
