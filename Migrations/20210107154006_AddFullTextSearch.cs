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
                "create or replace function product_tsvector_trigger() returns trigger language plpgsql as $$\n" +
                "declare store_title varchar(150);\n" +
                "begin\n" +
                "select \"Stores\".\"Title\"\n" +
                "into store_title\n" +
                "from \"Stores\"\n" +
                "where \"Stores\".\"Id\" = new.\"StoreId\"\n" +
                "limit 1;\n" +
                "new.\"DocumentWithWeights\" :=\n" +
                "setweight(to_tsvector(new.\"Title\"), 'A')\n" +
                "|| setweight(to_tsvector(coalesce(new.\"Description\" ,' ')), 'B')\n" +
                "|| setweight(to_tsvector(store_title), 'C');\n" +
                "return new;\n" +
                "end\n$$;");

            migrationBuilder.Sql(
                "create trigger tsvector_update_product_on_product\n" +
                "before insert or update\n" +
                "on \"Product\"\n" +
                "for each row\n" +
                "execute procedure product_tsvector_trigger();"
                );

            migrationBuilder.Sql(
                "create or replace function store_tsvector_trigger() returns trigger language plpgsql as $$\n" +
                "declare categories varchar(200);\n" +
                "begin\n" +
                "select string_agg(\"CategoryStore\".\"Title\", ' ')::varchar(200)\n" +
                "into categories from \"StoreCategories\"\n" +
                "join \"CategoryStore\" on \"CategoryStore\".\"Id\" = \"StoreCategories\".\"CategoryId\"\n" +
                "where \"StoreId\" = new.\"Id\" group by \"StoreId\";\n" +
                    "new.\"DocumentWithWeights\" :=\n" +
                "setweight(to_tsvector(new.\"Title\"), 'A')\n" +
                    "|| setweight(to_tsvector(coalesce(new.\"Description\" ,' ')), 'B')\n" +
                    "|| setweight(to_tsvector(coalesce(categories, ' ')), 'C');\n" +
                "return new;\n" +
                "end\n$$;\n"
            );

            migrationBuilder.Sql(
                "create trigger tsvector_update_store_on_store before insert or update\n" +
                "on \"Stores\" for each row execute procedure store_tsvector_trigger();"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop trigger tsvector_update_product_on_product on \"Product\"");
            migrationBuilder.Sql("drop function product_tsvector_trigger");

            migrationBuilder.Sql("drop trigger tsvector_update_store_on_store on \"Stores\"");
            migrationBuilder.Sql("drop function store_tsvector_trigger");
            
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
