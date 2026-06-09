using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowledgeAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AiStatus = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    AiError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    AiProcessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_AiStatus",
                table: "KnowledgeRecords",
                column: "AiStatus");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_Category",
                table: "KnowledgeRecords",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_CreatedAtUtc",
                table: "KnowledgeRecords",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_Status",
                table: "KnowledgeRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRecords_Type",
                table: "KnowledgeRecords",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeRecords");
        }
    }
}
