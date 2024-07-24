using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class GetPersons_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_GetAllPersons = @"CREATE PROCEDURE [dbo].[GetAllPersons]
                AS BEGIN
                    SELECT Id, PersonName, Email, DateOfBirth, Gender,
                    CountryId, Address, ReceiveNewsLetters FROM [dbo].
                    [Persons]
                END
                ";
            migrationBuilder.Sql(sp_GetAllPersons);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_GetAllPersons = @"DROP PROCEDURE [dbo].[GetAllPersons]";

            migrationBuilder.Sql(sp_GetAllPersons);
        }
    }
}
