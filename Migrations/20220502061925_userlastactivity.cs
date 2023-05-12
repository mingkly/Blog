using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace MyWeb.Migrations
{
    public partial class userlastactivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity_ArticleTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity_NotifyTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity_ReplyTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivity_ArticleTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastActivity_NotifyTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastActivity_ReplyTime",
                table: "AspNetUsers");
        }
    }
}
