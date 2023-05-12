using Microsoft.EntityFrameworkCore.Migrations;

namespace MyWeb.Migrations.VipNumber
{
    public partial class InitVipNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VipNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VipNumbers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VipNumbers");
        }
    }
}
