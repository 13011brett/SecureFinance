using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SecureFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RateLimit = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastHealthCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EncryptedSettings = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActive = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DataSourceId = table.Column<int>(type: "integer", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: false),
                    WasCached = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiRequests_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApiRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EncryptionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EncryptedAesKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    KeyIdentifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncryptionKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncryptionKeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DataSources",
                columns: new[] { "Id", "ApiKey", "BaseUrl", "IsActive", "LastHealthCheck", "Name", "RateLimit" },
                values: new object[,]
                {
                    { 1, null, "https://www.alphavantage.co/query", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Alpha Vantage", 5 },
                    { 2, null, "https://api.coingecko.com/api/v3", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CoinGecko", 50 },
                    { 3, null, "https://api.stlouisfed.org/fred", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FRED", 120 },
                    { 4, null, "https://api.exchangerate-api.com/v4", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exchange Rates API", 1500 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiRequests_DataSourceId",
                table: "ApiRequests",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiRequests_RequestedAt",
                table: "ApiRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApiRequests_UserId_RequestedAt",
                table: "ApiRequests",
                columns: new[] { "UserId", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_KeyIdentifier",
                table: "EncryptionKeys",
                column: "KeyIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EncryptionKeys_UserId_IsActive",
                table: "EncryptionKeys",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApiKey",
                table: "Users",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiRequests");

            migrationBuilder.DropTable(
                name: "EncryptionKeys");

            migrationBuilder.DropTable(
                name: "DataSources");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
