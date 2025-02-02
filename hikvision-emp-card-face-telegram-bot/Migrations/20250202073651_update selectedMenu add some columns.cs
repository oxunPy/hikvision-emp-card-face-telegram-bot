using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hikvision_emp_card_face_telegram_bot.Migrations
{
    /// <inheritdoc />
    public partial class updateselectedMenuaddsomecolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DishName",
                table: "SelectedMenus",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DishPrice",
                table: "SelectedMenus",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DishName",
                table: "SelectedMenus");

            migrationBuilder.DropColumn(
                name: "DishPrice",
                table: "SelectedMenus");
        }
    }
}
