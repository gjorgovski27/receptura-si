using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookingAssistantAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArticleDescription = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ArticleContent = table.Column<string>(type: "TEXT", nullable: false),
                    ArticleImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ImageMimeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.ArticleId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "News");
        }
    }
}
