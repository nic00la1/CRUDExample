using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddTransient<ResponseHeaderActionFilter>();

// it adds the MVC services to the container
        services.AddControllersWithViews(options =>
        {
            ILogger<ResponseHeaderActionFilter>? logger = services
                .BuildServiceProvider()
                .GetService<ILogger<ResponseHeaderActionFilter>>();

            options.Filters.Add(new ResponseHeaderActionFilter(logger)
            {
                Key = "My-Key-From-Global",
                Value = "My-Value-From-Global",
                Order = 2
            });
        });

// Add services into IoC container
        services.AddScoped<ICountriesRepository, CountriesRepository>();
        services.AddScoped<IPersonsRepository, PersonsRepository>();

        services.AddScoped<ICountriesGetterService, CountriesGetterService>();
        services.AddScoped<ICountriesExcelService, CountriesExcelService>();
        services.AddScoped<ICountriesAdderService, CountriesAdderService>();
        services.AddScoped<IPersonsSorterService, PersonsSorterService>();

        services
            .AddScoped<IPersonsGetterService,
                PersonsGetterServiceWithFewExcelFields>();
        services.AddScoped<PersonsGetterService,
            PersonsGetterService>();

        services.AddScoped<IPersonsAdderService, PersonsAdderService>();
        services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
        services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();


// Add HTTP logging services
        services.AddHttpLogging(options =>
        {
            // Configure logging options if needed
            options.LoggingFields = HttpLoggingFields.RequestProperties |
                HttpLoggingFields.ResponsePropertiesAndHeaders;
        });

// Conditionally register the DbContext based on the environment
        if (environment.IsEnvironment("Test"))
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        else
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString(
                        "DefaultConnection"));
            });

        services.AddTransient<PersonsListActionFilter>();

        return services;
    }
}
