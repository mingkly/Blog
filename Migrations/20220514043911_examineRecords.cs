using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWeb.Migrations
{
    public partial class examineRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamineRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerId = table.Column<long>(type: "bigint", nullable: false),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    CommentId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamineRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamineRecords_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamineRecords_AspNetUsers_ExaminerId",
                        column: x => x.ExaminerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamineRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamineRecords_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamineRecords_ArticleId",
                table: "ExamineRecords",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamineRecords_CommentId",
                table: "ExamineRecords",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamineRecords_ExaminerId",
                table: "ExamineRecords",
                column: "ExaminerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamineRecords_UserId",
                table: "ExamineRecords",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamineRecords");
        }
    }
}
