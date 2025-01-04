using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabaBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Locale = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    RewindSettings_MessagesThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    RewindSettings_EnableAutomaticReplies = table.Column<bool>(type: "INTEGER", nullable: false),
                    RewindSettings_EnablePingReplies = table.Column<bool>(type: "INTEGER", nullable: false),
                    RewindSettings_AutomaticRepliesCooldown = table.Column<int>(type: "INTEGER", nullable: false),
                    RewindSettings_CooldownCounter = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardSettings_RewardClanTag = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    RewardSettings_MessageTemplate_Content = table.Column<string>(type: "TEXT", nullable: true),
                    ReactionChampSettings_Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReactionChampSettings_EmoteId = table.Column<string>(type: "TEXT", nullable: true),
                    ReactionChampSettings_ReactionThreshold = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "DiscordAttachment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileUrl = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    DiscordMessageRewardSettingsGuildSettingsGuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Guilds_DeletedMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AuthorId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ReactionChampSettingsGuildSettingsGuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds_DeletedMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guilds_DeletedMessages_Guilds_ReactionChampSettingsGuildSettingsGuildId",
                        column: x => x.ReactionChampSettingsGuildSettingsGuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Guilds_Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AuthorId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RewindSettingsGuildSettingsGuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guilds_Messages_Guilds_RewindSettingsGuildSettingsGuildId",
                        column: x => x.RewindSettingsGuildSettingsGuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordAttachment_DiscordMessageRewardSettingsGuildSettingsGuildId",
                table: "DiscordAttachment",
                column: "DiscordMessageRewardSettingsGuildSettingsGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_DeletedMessages_ReactionChampSettingsGuildSettingsGuildId",
                table: "Guilds_DeletedMessages",
                column: "ReactionChampSettingsGuildSettingsGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_Messages_RewindSettingsGuildSettingsGuildId",
                table: "Guilds_Messages",
                column: "RewindSettingsGuildSettingsGuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordAttachment");

            migrationBuilder.DropTable(
                name: "Guilds_DeletedMessages");

            migrationBuilder.DropTable(
                name: "Guilds_Messages");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
