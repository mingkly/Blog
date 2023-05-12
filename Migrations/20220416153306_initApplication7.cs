using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWeb.Migrations
{
    public partial class initApplication7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CommentsCount",
                table: "Comments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VoteDownCount",
                table: "Comments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VoteUpCount",
                table: "Comments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FansCount",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FollowsCount",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CommentsCount",
                table: "Articles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FavouriteCount",
                table: "Articles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VoteDownCount",
                table: "Articles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VoteUpCount",
                table: "Articles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "VoteDownCount",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "VoteUpCount",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FansCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FollowsCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "FavouriteCount",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "VoteDownCount",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "VoteUpCount",
                table: "Articles");
        }
    }
}
