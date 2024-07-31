using Entities;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;
using Repositories;
using RepositoryContracts;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog(
    (HostBuilderContext context,
     IServiceProvider services,
     LoggerConfiguration loggerConfiguration
    ) =>
    {
        loggerConfiguration
            .ReadFrom
            .Configuration(context
                .Configuration) // read configuration settings from built-in IConfiguration
            .ReadFrom
            .Services(
                services); // Read out current app services and make them available to serilog
    });

builder.Services.AddControllersWithViews();

// Add services into IoC container
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonService, PersonsService>();

// Add HTTP logging services
builder.Services.AddHttpLogging(options =>
{
    // Configure logging options if needed
    options.LoggingFields = HttpLoggingFields.RequestProperties |
        HttpLoggingFields.ResponsePropertiesAndHeaders;
});

// Conditionally register the DbContext based on the environment
if (builder.Environment.IsEnvironment("Test"))
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseInMemoryDatabase("TestDatabase");
    });
else
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"));
    });

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();

if (builder.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseHttpLogging();

//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("info-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");

if (!builder.Environment.IsEnvironment("Test"))
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", "Rotativa");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program
{
} // make the auto-generated Program accessible programmatically
