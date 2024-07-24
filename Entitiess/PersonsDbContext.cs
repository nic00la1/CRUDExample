using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Entities;

public class PersonsDbContext : DbContext
{
    public PersonsDbContext(DbContextOptions<PersonsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Country> Countries { get; set; }
    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>().ToTable("Countries");
        modelBuilder.Entity<Person>().ToTable("Persons");

        // Seed data to countries table
        string countriesJson = File.ReadAllText("countries.json");

        List<Country> countries =
            JsonSerializer.Deserialize<List<Country>>(countriesJson)!;

        if (countries != null)
            foreach (Country country in countries)
                modelBuilder.Entity<Country>().HasData(country);

        // Seed data to persons table
        string personsJson = File.ReadAllText("persons.json");

        List<Person> persons =
            JsonSerializer.Deserialize<List<Person>>(personsJson)!;

        if (persons != null)
            foreach (Person person in persons)
                modelBuilder.Entity<Person>().HasData(person);

        // Fluent API
        modelBuilder.Entity<Person>()
            .Property(temp => temp.TIN).HasColumnName("TaxIdentificationNumber")
            .HasColumnType("varchar(8)").HasDefaultValue("ABC12345");

//        modelBuilder.Entity<Person>().HasIndex(temp => temp.TIN).IsUnique();

        modelBuilder.Entity<Person>()
            .HasCheckConstraint("CHK_TIN",
                "len([TaxIdentificationNumber]]) = 8");
    }

    public List<Person> sp_GetAllPersons()
    {
        return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
    }

    public int sp_InsertPerson(Person person)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new("@Id", person.Id),
            new("@PersonName", person.PersonName),
            new("@Email", person.Email),
            new("@DateOfBirth", person.DateOfBirth),
            new("@Gender", person.Gender),
            new("@CountryID", person.CountryID),
            new("@Address", person.Address),
            new("@ReceiveNewsLetters", person.ReceiveNewsLetters)
        };

        return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson]" +
            "@Id, @PersonName, @Email, @DateOfBirth, @Gender," +
            "@CountryID, @Address, @ReceiveNewsLetters", parameters);
    }

    public async Task<int> sp_DeletePersonAsync(Guid Id)
    {
        SqlParameter personIdParam = new("@Id", Id);
        return await Database.ExecuteSqlRawAsync("EXEC DeletePerson @Id",
            personIdParam);
    }

    public async Task<int> UpdatePersonAsync(Person person)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new("@Id", person.Id),
            new("@PersonName", person.PersonName),
            new("@Email", person.Email),
            new("@DateOfBirth", person.DateOfBirth),
            new("@Gender", person.Gender),
            new("@CountryID", person.CountryID),
            new("@Address", person.Address),
            new("@ReceiveNewsLetters", person.ReceiveNewsLetters)
        };

        return await Database.ExecuteSqlRawAsync(
            "EXEC UpdatePerson @Id, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters",
            parameters);
    }
}
