﻿namespace ServiceContracts.DTO;

using Entities;

/// <summary>
/// DTO class that is used as return type for most
/// of CountriesService methods
/// </summary>
public class CountryResponse
{
    public Guid CountryId { get; set; }
    public string? CountryName { get; set; }

    public static class CountryExtensions
    {
        public static CountryResponse ToCountryResponse(Country country)
        {
            return new CountryResponse()
            {
                CountryId = country.CountryId,
                CountryName = country.CountryName
            };
        }
    }
}
