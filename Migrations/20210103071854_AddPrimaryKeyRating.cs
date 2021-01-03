using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Migrations
{
    public partial class AddPrimaryKeyRating : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ratings_userlogin_storeid_uindex",
                table: "Ratings");

            migrationBuilder.AddPrimaryKey(
                name: "ratings_userlogin_storeid_uindex",
                table: "Ratings",
                columns: new[] { "UserLogin", "StoreId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "ratings_userlogin_storeid_uindex",
                table: "Ratings");

            migrationBuilder.CreateIndex(
                name: "ratings_userlogin_storeid_uindex",
                table: "Ratings",
                columns: new[] { "UserLogin", "StoreId" },
                unique: true);
        }
    }
}
