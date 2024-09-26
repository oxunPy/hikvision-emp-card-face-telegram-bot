using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hikvision_emp_card_face_telegram_bot.Migrations
{
    /// <inheritdoc />
    public partial class LunchmenuCurrentEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CurrentEdit",
                table: "LunchMenus",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_LunchMenus_DayOfWeek",
                table: "LunchMenus",
                column: "DayOfWeek",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LunchMenus_DayOfWeek",
                table: "LunchMenus");

            migrationBuilder.DropColumn(
                name: "CurrentEdit",
                table: "LunchMenus");
        }
    }
}
