# 🌍 Tourism Telegram Bot

Professional Telegram bot for tourism advertisements with Clean Architecture in C# / .NET 8.

## Architecture

```
TelegramBot.Domain          ← Entities, Enums, Exceptions (no dependencies)
TelegramBot.Application     ← Interfaces, Services, DTOs, Mappings
TelegramBot.Infrastructure  ← EF Core (SQLite), Repository implementations
TelegramBot.Bot             ← Telegram polling, handlers, background service
TelegramBot.Api             ← REST API (Swagger UI)
```

## Quick Start

### 1. Configure the bot
Edit `src/TelegramBot.Bot/appsettings.json`:
```json
{
  "BotSettings": {
    "BotToken": "YOUR_TOKEN_FROM_BOTFATHER",
    "AdminIds": [ 123456789 ],
    "AdminUsername": "Madina_Centraltur",
    "ChannelId": "@your_channel"
  }
}
```

Get your `AdminIds` by messaging [@userinfobot](https://t.me/userinfobot) on Telegram.

### 2. Run the bot
```bash
dotnet run --project src/TelegramBot.Bot
```

### 3. Run the REST API (optional)
```bash
dotnet run --project src/TelegramBot.Api
# Open http://localhost:5000/swagger
```

## Admin Commands
| Command | Description |
|---------|-------------|
| `/start` | Admin panel menu |
| `/create_ad` | Start creating a new advertisement |
| `/my_ads` | View all your advertisements |
| `/stats` | View analytics & statistics |

## User Flow
1. User opens bot → sees active advertisements
2. Clicks **"Qiziqaman ✈️"** button
3. Telegram opens admin chat with pre-filled message:
   > *"Assalomu alaykum! Men "TOUR NAME" bo'yicha qiziqaman..."*
4. User hits Send → Admin gets the lead

## Ad Creation Flow (Admin)
```
/create_ad
 → Enter title
 → Upload photo/video (or skip)
 → Enter direction (Toshkent — Antaliya)
 → Enter transport info (UZAIRWAYS Boeing 787)
 → Enter departure date (18.05.2025) or skip
 → Enter nights count or skip
 → Add hotels (Name | Price | Extra)  ← repeat for each hotel
 → Enter contact info
 → Confirm → Ad saved!
 → Publish to channel with 📢 button
```

## Tech Stack
- **Runtime**: .NET 8
- **Bot Library**: Telegram.Bot 22.x
- **Database**: SQLite via EF Core 8
- **Architecture**: Clean Architecture + Repository Pattern
- **DI**: Microsoft.Extensions.DependencyInjection

## Analytics Tracked
- 👁 **Views** — ad was seen
- 👆 **Clicks** — "Qiziqaman" button clicked
- 💬 **Messages Sent** — user actually messaged admin
- 📊 **CTR** and **Conversion Rate** calculated automatically
