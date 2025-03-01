using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PaymentsMS.Migrations
{
    /// <inheritdoc />
    public partial class updatedonations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    stripe_session_id = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_type = table.Column<string>(type: "text", nullable: false),
                    transaction_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    transaction_completed_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    transaction_status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_transaction = table.Column<int>(type: "integer", nullable: false),
                    percent = table.Column<decimal>(type: "numeric", nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_comissions_transactions_id_transaction",
                        column: x => x.id_transaction,
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "donations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_tournament = table.Column<int>(type: "integer", nullable: false),
                    id_transaction = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_donations", x => x.id);
                    table.ForeignKey(
                        name: "FK_donations_transactions_id_transaction",
                        column: x => x.id_transaction,
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comissions_id_transaction",
                table: "comissions",
                column: "id_transaction",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_donations_id_transaction",
                table: "donations",
                column: "id_transaction",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comissions");

            migrationBuilder.DropTable(
                name: "donations");

            migrationBuilder.DropTable(
                name: "transactions");
        }
    }
}
