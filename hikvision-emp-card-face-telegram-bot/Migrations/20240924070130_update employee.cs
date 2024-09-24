using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hikvision_emp_card_face_telegram_bot.Migrations
{
    /// <inheritdoc />
    public partial class updateemployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HikCardId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsPinVerified",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PinCode",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HikCardId",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinVerified",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PinCode",
                table: "Employees",
                type: "text",
                nullable: true);
        }
    }
}
