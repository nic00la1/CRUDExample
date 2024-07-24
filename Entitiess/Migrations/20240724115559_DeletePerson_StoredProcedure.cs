using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class DeletePerson_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_DeletePerson = @"CREATE PROCEDURE [dbo].[DeletePerson]
                (@PersonId uniqueidentifier)
                AS BEGIN
                   DELETE FROM [dbo].[Persons]
                   WHERE Id = @PersonId
                END
                ";
            migrationBuilder.Sql(sp_DeletePerson);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_DeletePerson = @"DROP PROCEDURE [dbo].[DeletePerson]";
            migrationBuilder.Sql(sp_DeletePerson);
        }
    }
}
