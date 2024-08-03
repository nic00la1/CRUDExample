using CRUDExample.Filters.ActionFilters;
using CRUDExample.Middleware;
using CRUDExample.StartupExtensions;
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

builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();

if (builder.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseHttpLogging();

if (!builder.Environment.IsEnvironment("Test"))
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", "Rotativa");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program
{
} // make the auto-generated Program accessible programmatically
