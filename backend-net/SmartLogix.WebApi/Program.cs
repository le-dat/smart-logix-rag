using Microsoft.EntityFrameworkCore;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Repositories;
using SmartLogix.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent infinite reference loops during JSON serialization of related tables
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SmartLogix Integration Gateway API",
        Version = "v1",
        Description = "ASP.NET Core 8 Gateway API handling CRUD operations, database transactions, and proxy requests for AI features."
    });
});

// Configure EF Core with MS SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmartLogixDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => 
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    )
);

// Register Repositories (Dependency Injection)
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// Register Services (Dependency Injection)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLogix API v1");
        c.RoutePrefix = "docs"; // Best practice: Rút gọn URL thành http://localhost:5000/docs
    });
}

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

// Automate Database Schema creation and Seeding on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SmartLogixDbContext>();
        app.Logger.LogInformation("Database auto-migrating and seeding...");
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while creating or seeding the database.");
    }
}

app.Run();
