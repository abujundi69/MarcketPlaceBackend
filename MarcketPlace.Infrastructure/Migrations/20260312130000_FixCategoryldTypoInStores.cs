using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcketPlace.Infrastructure.Migrations
{
    /// <summary>
    /// Fixes typo: renames Categoryld to CategoryId in Stores table if the wrong column exists.
    /// </summary>
    public partial class FixCategoryldTypoInStores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix typo: Categoryld -> CategoryId (if Categoryld exists)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Stores' AND COLUMN_NAME = 'Categoryld'
                )
                BEGIN
                    EXEC sp_rename 'Stores.Categoryld', 'CategoryId', 'COLUMN';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert: CategoryId -> Categoryld (only if we need to rollback)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Stores' AND COLUMN_NAME = 'CategoryId'
                )
                AND NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Stores' AND COLUMN_NAME = 'Categoryld'
                )
                BEGIN
                    EXEC sp_rename 'Stores.CategoryId', 'Categoryld', 'COLUMN';
                END
            ");
        }
    }
}
