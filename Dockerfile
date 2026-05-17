# ── Build stage ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Faqat csproj fayllarni copy qilamiz (layer cache uchun)
COPY ["src/TelegramBot.Domain/TelegramBot.Domain.csproj",           "TelegramBot.Domain/"]
COPY ["src/TelegramBot.Application/TelegramBot.Application.csproj", "TelegramBot.Application/"]
COPY ["src/TelegramBot.Infrastructure/TelegramBot.Infrastructure.csproj", "TelegramBot.Infrastructure/"]
COPY ["src/TelegramBot.Bot/TelegramBot.Bot.csproj",                 "TelegramBot.Bot/"]

RUN dotnet restore "TelegramBot.Bot/TelegramBot.Bot.csproj"

# Barcha source code ni copy qilamiz
COPY src/TelegramBot.Domain/      TelegramBot.Domain/
COPY src/TelegramBot.Application/ TelegramBot.Application/
COPY src/TelegramBot.Infrastructure/ TelegramBot.Infrastructure/
COPY src/TelegramBot.Bot/         TelegramBot.Bot/

RUN dotnet publish "TelegramBot.Bot/TelegramBot.Bot.csproj" \
    -c Release -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# DB fayl saqlanadigan papka
RUN mkdir -p /data

COPY --from=build /app/publish .

# DB yo'lini environment variable sifatida beramiz
ENV ConnectionStrings__DefaultConnection="Data Source=/data/tourism_bot.db"
ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TelegramBot.Bot.dll"]
