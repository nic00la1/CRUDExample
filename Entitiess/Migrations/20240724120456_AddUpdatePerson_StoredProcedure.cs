using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatePerson_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE UpdatePerson
                    @Id UNIQUEIDENTIFIER,
                    @PersonName NVARCHAR(100),
                    @Email NVARCHAR(100),
                    @DateOfBirth DATETIME,
                    @Gender NVARCHAR(10),
                    @CountryID UNIQUEIDENTIFIER,
                    @Address NVARCHAR(200),
                    @ReceiveNewsLetters BIT
                AS
                BEGIN
                    UPDATE Persons
                    SET 
                        PersonName = @PersonName,
                        Email = @Email,
                        DateOfBirth = @DateOfBirth,
                        Gender = @Gender,
                        CountryID = @CountryID,
                        Address = @Address,
                        ReceiveNewsLetters = @ReceiveNewsLetters
                    WHERE Id = @Id;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE UpdatePerson");
        }
    }
}
