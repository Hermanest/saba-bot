using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabaBot.Migrations
{
    /// <inheritdoc />
    public partial class RewardsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordAttachment");

            migrationBuilder.RenameColumn(
                name: "RewardSettings_RewardClanTag",
                table: "Guilds",
                newName: "RewardSettings_ClanTag");

            migrationBuilder.RenameColumn(
                name: "RewardSettings_MessageTemplate_Content",
                table: "Guilds",
                newName: "RewardSettings_MessageText");

            migrationBuilder.AddColumn<string>(
                name: "RewardSettings_FilePaths",
                table: "Guilds",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RewardSettings_FilePaths",
                table: "Guilds");

            migrationBuilder.RenameColumn(
                name: "RewardSettings_MessageText",
                table: "Guilds",
                newName: "RewardSettings_MessageTemplate_Content");

            migrationBuilder.RenameColumn(
                name: "RewardSettings_ClanTag",
                table: "Guilds",
                newName: "RewardSettings_RewardClanTag");

            migrationBuilder.CreateTable(
                name: "DiscordAttachment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordMessageRewardSettingsGuildSettingsGuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordAttachment_Guilds_DiscordMessageRewardSettingsGuildSettingsGuildId",
                        column: x => x.DiscordMessageRewardSettingsGuildSettingsGuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordAttachment_DiscordMessageRewardSettingsGuildSettingsGuildId",
                table: "DiscordAttachment",
                column: "DiscordMessageRewardSettingsGuildSettingsGuildId");
        }
    }
}
