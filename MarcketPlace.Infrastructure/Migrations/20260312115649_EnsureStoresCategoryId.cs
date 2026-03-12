using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarcketPlace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureStoresCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure Stores has CategoryId: rename Categoryld if typo exists, or add column if missing
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Stores' AND COLUMN_NAME = 'Categoryld')
                BEGIN
                    EXEC sp_rename 'Stores.Categoryld', 'CategoryId', 'COLUMN';
                END
                ELSE IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Stores' AND COLUMN_NAME = 'CategoryId')
                BEGIN
                    ALTER TABLE Stores ADD CategoryId int NULL;
                    CREATE NONCLUSTERED INDEX IX_Stores_CategoryId ON Stores(CategoryId);
                    ALTER TABLE Stores ADD CONSTRAINT FK_Stores_Categories_CategoryId 
                        FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down - column should stay
        }
    }
}
