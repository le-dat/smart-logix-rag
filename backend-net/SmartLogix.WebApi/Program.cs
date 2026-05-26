using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Repositories;
using SmartLogix.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Controllers & JSON ─────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ─── Swagger / OpenAPI ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartLogix Integration Gateway API",
        Version = "v1",
        Description = "ASP.NET Core 8 Gateway API: JWT Auth, CRUD, and secure AI proxy forwarding to Python FastAPI."
    });

    // Add Bearer Token support in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: Bearer eyJhbGciOi..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Database (EF Core / SQL Server) ────────────────────────────────────────
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

// ─── JWT Authentication ──────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SmartLogix_Default_JWT_SecretKey_2026_Must_Be_32_Chars!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartLogixGateway";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SmartLogixClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ─── HttpClient for Python AI Engine Proxy ───────────────────────────────────
var pythonBaseUrl = builder.Configuration["PythonAiService:BaseUrl"] ?? "http://ai-engine-python:8000";
builder.Services.AddHttpClient("FastApi", client =>
{
    client.BaseAddress = new Uri(pythonBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(120); // allow long streaming responses
});

// ─── Repositories (DI) ──────────────────────────────────────────────────────
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// ─── Services (DI) ──────────────────────────────────────────────────────────
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ─── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:3000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

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
        app.Logger.LogInformation("Database auto-migrating and seeding...");
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while creating or seeding the database.");
    }
}

app.Run();
