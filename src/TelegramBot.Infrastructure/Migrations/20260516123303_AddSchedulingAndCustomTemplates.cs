using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingAndCustomTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminUsername",
                table: "Advertisements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomText",
                table: "Advertisements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Advertisements",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPublishTime",
                table: "Advertisements",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminUsername",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "CustomText",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "ScheduledPublishTime",
                table: "Advertisements");
        }
    }
}
