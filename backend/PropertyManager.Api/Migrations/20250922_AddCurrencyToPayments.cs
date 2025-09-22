using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyManager.Api.Migrations
{
    public partial class AddCurrencyToPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "RentalContracts",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "RentPayments",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            // Backfill existing data explicitly if required - here we set existing rows to XAF (2)
            migrationBuilder.Sql("UPDATE \"RentalContracts\" SET \"Currency\" = 2 WHERE \"Currency\" IS NULL OR \"Currency\" <> 2;");
            migrationBuilder.Sql("UPDATE \"RentPayments\" SET \"Currency\" = 2 WHERE \"Currency\" IS NULL OR \"Currency\" <> 2;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "RentPayments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "RentalContracts");
        }
    }
}
