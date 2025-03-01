using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentsMS.Migrations
{
    /// <inheritdoc />
    public partial class fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_name",
                table: "transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_user",
                table: "donations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id_name",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "id_user",
                table: "donations");
        }
    }
}
