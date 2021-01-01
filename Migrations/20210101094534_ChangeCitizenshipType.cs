using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Migrations
{
    public partial class ChangeCitizenshipType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "couriers_citizenshipid_passportnumber_uindex",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "CitizenshipId",
                table: "Couriers");

            migrationBuilder.AddColumn<string>(
                name: "Citizenship",
                table: "Couriers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "couriers_citizenshipid_passportnumber_uindex",
                table: "Couriers",
                columns: new[] { "Citizenship", "PassportNumber" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "couriers_citizenshipid_passportnumber_uindex",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "Citizenship",
                table: "Couriers");

            migrationBuilder.AddColumn<int>(
                name: "CitizenshipId",
                table: "Couriers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "couriers_citizenshipid_passportnumber_uindex",
                table: "Couriers",
                columns: new[] { "CitizenshipId", "PassportNumber" },
                unique: true);
        }
    }
}
