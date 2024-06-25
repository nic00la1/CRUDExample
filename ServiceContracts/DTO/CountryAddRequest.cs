using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContracts.DTO;

/// <summary>
/// DTO class for adding a new country
/// </summary>
public class CountryAddRequest
{
    public string? CountryName { get; set; }

    public Country ToCountry()
    {
        return new Country()
        {
            CountryName = CountryName
        };
    }
}
