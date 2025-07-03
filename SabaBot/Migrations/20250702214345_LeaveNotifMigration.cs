using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabaBot.Migrations
{
    /// <inheritdoc />
    public partial class LeaveNotifMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "LeaveNotifSettings_ChannelId",
                table: "Guilds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<bool>(
                name: "LeaveNotifSettings_Enabled",
                table: "Guilds",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaveNotifSettings_ChannelId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "LeaveNotifSettings_Enabled",
                table: "Guilds");
        }
    }
}
