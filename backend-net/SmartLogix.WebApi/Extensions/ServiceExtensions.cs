using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Repositories;
using SmartLogix.WebApi.Services;

namespace SmartLogix.WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddControllersWithJsonOptions(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });
            return services;
        }

        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SmartLogix Integration Gateway API",
                    Version = "v1",
                    Description = "ASP.NET Core 8 Gateway API: JWT Auth, CRUD, and secure AI proxy forwarding to Python FastAPI."
                });

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
            return services;
        }

        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<SmartLogixDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    )
                )
            );
            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "SmartLogixGateway";
            var jwtAudience = configuration["Jwt:Audience"] ?? "SmartLogixClients";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
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
            return services;
        }

        public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            var pythonBaseUrl = configuration["PythonAiService:BaseUrl"] ?? "http://ai-engine-python:8000";
            services.AddHttpClient("FastApi", client =>
            {
                client.BaseAddress = new Uri(pythonBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(120);
            });
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Security Utilities
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // Repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();

            // Business Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IShipmentService, ShipmentService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAiService, AiService>();

            return services;
        }
    }
}
