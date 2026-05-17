using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBotUserIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BotUsers_IsAdmin_LastBroadcastSentAt",
                table: "BotUsers",
                columns: new[] { "IsAdmin", "LastBroadcastSentAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BotUsers_IsAdmin_LastBroadcastSentAt",
                table: "BotUsers");
        }
    }
}
