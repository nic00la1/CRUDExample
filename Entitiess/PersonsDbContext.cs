using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Entities;

public class PersonsDbContext : DbContext
{
    public DbSet<Country> Countries { get; set; }
    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>().ToTable("Countries");
        modelBuilder.Entity<Person>().ToTable("Persons");
    }
}
