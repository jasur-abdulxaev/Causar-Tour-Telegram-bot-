Write-Host "Starting Tourism Telegram Bot System..." -ForegroundColor Cyan

# Start API in background
Start-Process "dotnet" -ArgumentList "run --project src/TelegramBot.Api" -NoNewWindow -PassThru

# Start Bot in background
Start-Process "dotnet" -ArgumentList "run --project src/TelegramBot.Bot" -NoNewWindow -PassThru

# Start Blazor Web UI in background
Start-Process "dotnet" -ArgumentList "run --project src/AdminPanel.Web" -NoNewWindow -PassThru

Write-Host "All services started! Press Ctrl+C to exit (you might need to manually kill dotnet processes later)." -ForegroundColor Green
