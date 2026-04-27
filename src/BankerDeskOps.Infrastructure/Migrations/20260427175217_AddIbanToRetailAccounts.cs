using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankerDeskOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIbanToRetailAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "RetailAccounts",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iban",
                table: "RetailAccounts");
        }
    }
}
