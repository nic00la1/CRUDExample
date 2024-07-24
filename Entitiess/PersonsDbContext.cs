﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    }

    public List<Person> sp_GetAllPersons()
    {
        return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
    }
}
