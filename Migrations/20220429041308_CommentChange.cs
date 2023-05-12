using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWeb.Migrations
{
    public partial class CommentChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BelongToCommentId",
                table: "Comments",
                type: "bigint",
                nullable: true);



            migrationBuilder.CreateIndex(
                name: "IX_Comments_BelongToCommentId",
                table: "Comments",
                column: "BelongToCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_BelongToCommentId",
                table: "Comments",
                column: "BelongToCommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_BelongToCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_BelongToCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "BelongToCommentId",
                table: "Comments");

        }
    }
}
