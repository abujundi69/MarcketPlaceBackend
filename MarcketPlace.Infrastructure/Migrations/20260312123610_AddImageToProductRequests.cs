using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcketPlace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToProductRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('ProductRequests') AND name = 'Image'
                )
                BEGIN
                    ALTER TABLE ProductRequests ADD Image varbinary(max) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('ProductRequests') AND name = 'Image'
                )
                BEGIN
                    ALTER TABLE ProductRequests DROP COLUMN Image;
                END
            ");
        }
    }
}
