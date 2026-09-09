using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlanticCity.Authentication.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuthenticationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auth_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "auth_refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_auth_refresh_token_auth_refresh_token_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalTable: "auth_refresh_token",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_auth_refresh_token_auth_user_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auth_refresh_token_expires_at",
                table: "auth_refresh_token",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_auth_refresh_token_replaced_by_token_id",
                table: "auth_refresh_token",
                column: "replaced_by_token_id");

            migrationBuilder.CreateIndex(
                name: "ix_auth_refresh_token_user_id",
                table: "auth_refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_auth_refresh_token_hash",
                table: "auth_refresh_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_auth_user_email",
                table: "auth_user",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auth_refresh_token");

            migrationBuilder.DropTable(
                name: "auth_user");
        }
    }
}
