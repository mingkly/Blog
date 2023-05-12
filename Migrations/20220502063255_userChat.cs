using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWeb.Migrations
{
    public partial class userChat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatInfos_ChatToUserName",
                table: "ChatInfos");

            migrationBuilder.DropColumn(
                name: "ChatToUserName",
                table: "ChatInfos");

            migrationBuilder.AddColumn<long>(
                name: "ChatToUserId",
                table: "ChatInfos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ChatInfos_ChatToUserId",
                table: "ChatInfos",
                column: "ChatToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatInfos_AspNetUsers_ChatToUserId",
                table: "ChatInfos",
                column: "ChatToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatInfos_AspNetUsers_ChatToUserId",
                table: "ChatInfos");

            migrationBuilder.DropIndex(
                name: "IX_ChatInfos_ChatToUserId",
                table: "ChatInfos");

            migrationBuilder.DropColumn(
                name: "ChatToUserId",
                table: "ChatInfos");

            migrationBuilder.AddColumn<string>(
                name: "ChatToUserName",
                table: "ChatInfos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatInfos_ChatToUserName",
                table: "ChatInfos",
                column: "ChatToUserName");
        }
    }
}
