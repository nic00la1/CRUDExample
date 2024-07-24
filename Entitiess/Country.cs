using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Domain model for Country
/// </summary>
public class Country
{
    [Key] public Guid CountryId { get; set; }

    public string? CountryName { get; set; }

    [ForeignKey("CountryId")]
    public virtual ICollection<Person>? Persons { get; set; }
}
