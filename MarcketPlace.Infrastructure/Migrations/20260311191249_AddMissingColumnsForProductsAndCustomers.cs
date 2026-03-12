using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcketPlace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsForProductsAndCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Products",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultAddressText",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultDeliveryZoneId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultLatitude",
                table: "Customers",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultLongitude",
                table: "Customers",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LocationUpdatedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DefaultDeliveryZoneId",
                table: "Customers",
                column: "DefaultDeliveryZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_DeliveryZones_DefaultDeliveryZoneId",
                table: "Customers",
                column: "DefaultDeliveryZoneId",
                principalTable: "DeliveryZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_DeliveryZones_DefaultDeliveryZoneId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DefaultDeliveryZoneId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DefaultAddressText",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DefaultDeliveryZoneId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DefaultLatitude",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DefaultLongitude",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LocationUpdatedAt",
                table: "Customers");
        }
    }
}
