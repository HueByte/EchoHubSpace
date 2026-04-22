using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EchoHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Hosts = table.Column<string>(type: "jsonb", nullable: false),
                    UserCount = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    ClaimTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ClaimTokenHash",
                table: "Servers",
                column: "ClaimTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Hosts",
                table: "Servers",
                column: "Hosts")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Tags",
                table: "Servers",
                column: "Tags")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
