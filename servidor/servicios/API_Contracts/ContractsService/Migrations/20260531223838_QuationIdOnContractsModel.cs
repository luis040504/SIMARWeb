using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractsService.Migrations
{
    /// <inheritdoc />
    public partial class QuationIdOnContractsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuotationId",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "Contracts");
        }
    }
}
