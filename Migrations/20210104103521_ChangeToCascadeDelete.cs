using Microsoft.EntityFrameworkCore.Migrations;

namespace Delivery.Migrations
{
    public partial class ChangeToCascadeDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "order_address_id_fk",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "order_stores_id_fk",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "order_users_login_fk",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "ratings_users_login_fk",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "stores_users_login_fk",
                table: "Stores");

            migrationBuilder.AddForeignKey(
                name: "order_address_id_fk",
                table: "Order",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "order_stores_id_fk",
                table: "Order",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "order_users_login_fk",
                table: "Order",
                column: "UserLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "ratings_users_login_fk",
                table: "Ratings",
                column: "UserLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "stores_users_login_fk",
                table: "Stores",
                column: "OwnerLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "order_address_id_fk",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "order_stores_id_fk",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "order_users_login_fk",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "ratings_users_login_fk",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "stores_users_login_fk",
                table: "Stores");

            migrationBuilder.AddForeignKey(
                name: "order_address_id_fk",
                table: "Order",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "order_stores_id_fk",
                table: "Order",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "order_users_login_fk",
                table: "Order",
                column: "UserLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "ratings_users_login_fk",
                table: "Ratings",
                column: "UserLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "stores_users_login_fk",
                table: "Stores",
                column: "OwnerLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
