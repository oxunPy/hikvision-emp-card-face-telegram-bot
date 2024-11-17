using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hikvision_emp_card_face_telegram_bot.Migrations
{
    /// <inheritdoc />
    public partial class VisitedDateofEmplooyee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VisitedDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisitedDate",
                table: "Employees");
        }
    }
}
