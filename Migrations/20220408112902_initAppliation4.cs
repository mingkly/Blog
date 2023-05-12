using Microsoft.EntityFrameworkCore.Migrations;

namespace MyWeb.Migrations
{
    public partial class initAppliation4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserViews",
                table: "UserViews");

            migrationBuilder.AlterColumn<long>(
                name: "ArticleId",
                table: "UserViews",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserViews",
                table: "UserViews",
                columns: new[] { "ArticleId", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserViews",
                table: "UserViews");

            migrationBuilder.AlterColumn<long>(
                name: "ArticleId",
                table: "UserViews",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserViews",
                table: "UserViews",
                column: "Id");
        }
    }
}
