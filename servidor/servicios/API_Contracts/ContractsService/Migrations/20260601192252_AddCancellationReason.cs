using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractsService.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Contracts]') 
                    AND name = 'CancellationReason'
                )
                BEGIN
                    ALTER TABLE [Contracts] ADD [CancellationReason] nvarchar(max) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Contracts]') 
                    AND name = 'CancellationReason'
                )
                BEGIN
                    ALTER TABLE [Contracts] DROP COLUMN [CancellationReason];
                END
            ");
        }
    }
}
