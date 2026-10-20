using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mimo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserProfiles_And_UserStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalXp",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DailyGoalMinutes = table.Column<int>(type: "integer", nullable: false),
                    RiskSurveyResult = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStats",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Hearts = table.Column<int>(type: "integer", nullable: false),
                    HeartsUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    LastActiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StreakFreezeCount = table.Column<int>(type: "integer", nullable: false),
                    IsPro = table.Column<bool>(type: "boolean", nullable: false),
                    ProExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStats", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "UserStats");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalXp",
                table: "Users");
        }
    }
}
