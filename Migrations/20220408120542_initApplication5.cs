using Microsoft.EntityFrameworkCore.Migrations;

namespace MyWeb.Migrations
{
    public partial class initApplication5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
    name: "Avatar",
    table: "AspNetUsers",
    type: "nvarchar(max)",
    nullable: true);
            migrationBuilder.AddColumn<string>(
name: "Background",
table: "AspNetUsers",
type: "nvarchar(max)",
nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
    name: "Avatar",
    table: "AspNetUsers");
            migrationBuilder.DropColumn(
name: "Background",
table: "AspNetUsers");
        }
    }
}
