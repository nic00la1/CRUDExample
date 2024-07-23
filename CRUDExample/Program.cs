using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Add services into IoC container
builder.Services.AddSingleton<ICountriesService, CountriesService>();
builder.Services.AddSingleton<IPersonService, PersonsService>();

builder.Services.AddDbContext<PersonsDbContext>(options =>
{
    options.UseSqlServer();
});

WebApplication app = builder.Build();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
