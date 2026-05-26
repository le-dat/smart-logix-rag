using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Extensions;
using SmartLogix.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Register Services (via ServiceExtensions) ──────────────────────────────
builder.Services.AddControllersWithJsonOptions();
builder.Services.AddSwaggerServices();
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddHttpClientServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLogix API v1");
        c.RoutePrefix = "docs";
    });
}

app.UseCors("CorsPolicy");

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─── Auto-Migrate and Seed Database ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SmartLogixDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        app.Logger.LogInformation("Database auto-migrating and seeding...");
        DbInitializer.Initialize(context, passwordHasher);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while creating or seeding the database.");
    }
}

app.Run();
