using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

namespace Delivery.Migrations
{
    public partial class AddFullTextSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "DocumentWithWeights",
                table: "Stores",
                type: "tsvector",
                nullable: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "DocumentWithWeights",
                table: "Product",
                type: "tsvector",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_DocumentWithWeights",
                table: "Stores",
                column: "DocumentWithWeights")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Product_DocumentWithWeights",
                table: "Product",
                column: "DocumentWithWeights")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.Sql(
                @"CREATE TRIGGER");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stores_DocumentWithWeights",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Product_DocumentWithWeights",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "DocumentWithWeights",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "DocumentWithWeights",
                table: "Product");
        }
    }
}
