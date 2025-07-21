using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Depot.Repository.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "repo");

            migrationBuilder.CreateTable(
                name: "Policy",
                schema: "repo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Immutable = table.Column<bool>(type: "boolean", nullable: false),
                    RetentionDays = table.Column<int>(type: "integer", nullable: true),
                    RetentionVersions = table.Column<int>(type: "integer", nullable: true),
                    FileTypes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                schema: "repo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Repositories_Policy_PolicyId",
                        column: x => x.PolicyId,
                        principalSchema: "repo",
                        principalTable: "Policy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "artifacts",
                schema: "repo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(127)", maxLength: 127, nullable: false),
                    Extension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Repository = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_artifacts_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalSchema: "repo",
                        principalTable: "Repositories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_Hash",
                schema: "repo",
                table: "artifacts",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_Location",
                schema: "repo",
                table: "artifacts",
                column: "Location",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_Repository_Name",
                schema: "repo",
                table: "artifacts",
                columns: new[] { "Repository", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_RepositoryId",
                schema: "repo",
                table: "artifacts",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_PolicyId",
                schema: "repo",
                table: "Repositories",
                column: "PolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artifacts",
                schema: "repo");

            migrationBuilder.DropTable(
                name: "Repositories",
                schema: "repo");

            migrationBuilder.DropTable(
                name: "Policy",
                schema: "repo");
        }
    }
}
