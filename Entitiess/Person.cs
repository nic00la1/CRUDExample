using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities;

/// <summary>
/// Person domain model class
/// </summary>
public class Person
{
    [Key] public Guid Id { get; set; }

    // nvarchar(max)
    [StringLength(40)]
    // [Required] 
    public string? PersonName { get; set; }

    [StringLength(40)] public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [StringLength(10)] public string? Gender { get; set; }

    // unique identifier
    public Guid? CountryId { get; set; }

    [StringLength(200)] public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }

    public string? TIN { get; set; }

    public Country? Country { get; set; }

    public override string ToString()
    {
        return
            $"Person ID: {Id}, Person Name: {PersonName}, Email: {Email}, Date of Birth: {DateOfBirth?.ToString("MM/dd/yyyy")}, Gender: {Gender}, Country ID: {CountryId}, Country: {Country?.CountryName}, Address: {Address}, Receive News Letters: {ReceiveNewsLetters}";
    }
}
