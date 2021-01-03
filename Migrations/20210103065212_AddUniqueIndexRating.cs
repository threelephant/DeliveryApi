using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Migrations
{
    public partial class AddUniqueIndexRating : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ratings_userlogin_storeid_uindex",
                table: "Ratings",
                columns: new[] { "UserLogin", "StoreId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ratings_userlogin_storeid_uindex",
                table: "Ratings");
        }
    }
}
