using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SmartLogix.WebApi.Models;
using Route = SmartLogix.WebApi.Models.Route;


namespace SmartLogix.WebApi.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SmartLogixDbContext context)
        {
            // Auto-create database schema if it doesn't exist
            context.Database.EnsureCreated();

            // Look for any customers to check if already seeded
            if (context.Customers.Any())
            {
                return; // Database has been seeded
            }

            Console.WriteLine("Seeding Database...");

            // 1. Seed Customers
            var customers = new[]
            {
                new Customer { Name = "Dimerco Logistics Vietnam", Email = "operations.vn@dimerco.com", Phone = "+84-28-3997-2222" },
                new Customer { Name = "Samsung Electronics Vina", Email = "supply.chain@samsung.com", Phone = "+84-24-3831-5555" },
                new Customer { Name = "Apple Supply Chain Operations", Email = "logistics.global@apple.com", Phone = "+1-408-996-1010" },
                new Customer { Name = "Foxconn Technology Group", Email = "shipping@foxconn.com", Phone = "+886-2-2268-3466" },
                new Customer { Name = "Unilever International", Email = "inbound.shipping@unilever.com", Phone = "+44-20-7822-5252" }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();

            // 2. Seed Routes
            var routes = new[]
            {
                new Route { Source = "Tan Son Nhat (SGN), Vietnam", Destination = "Changi (SIN), Singapore", AverageDuration = 24 }, // 1 day
                new Route { Source = "Noi Bai (HAN), Vietnam", Destination = "Incheon (ICN), South Korea", AverageDuration = 48 }, // 2 days
                new Route { Source = "Taoyuan (TPE), Taiwan", Destination = "Noi Bai (HAN), Vietnam", AverageDuration = 36 }, // 1.5 days
                new Route { Source = "Shanghai Pudong (PVG), China", Destination = "Los Angeles (LAX), USA", AverageDuration = 120 }, // 5 days
                new Route { Source = "Noi Bai (HAN), Vietnam", Destination = "Frankfurt (FRA), Germany", AverageDuration = 96 } // 4 days
            };

            context.Routes.AddRange(routes);
            context.SaveChanges();

            // 3. Seed Shipments
            var shipments = new[]
            {
                new Shipment
                {
                    TrackingNo = "DMCO-VN-20260001",
                    Sender = "Samsung Bac Ninh Plant",
                    Receiver = "Samsung Hub Singapore",
                    CustomerId = customers[1].Id,
                    RouteId = routes[0].Id,
                    Weight = 4250.50m,
                    Status = "InTransit",
                    CreatedAt = DateTime.UtcNow.AddHours(-12)
                },
                new Shipment
                {
                    TrackingNo = "DMCO-US-20260002",
                    Sender = "Foxconn SGN Industrial Park",
                    Receiver = "Apple Cupertino HQ",
                    CustomerId = customers[2].Id,
                    RouteId = routes[3].Id,
                    Weight = 12500.00m,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                },
                new Shipment
                {
                    TrackingNo = "DMCO-KR-20260003",
                    Sender = "Dimerco Logistics HAN WH",
                    Receiver = "Incheon Cargo Center",
                    CustomerId = customers[0].Id,
                    RouteId = routes[1].Id,
                    Weight = 850.00m,
                    Status = "Delivered",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Shipment
                {
                    TrackingNo = "DMCO-EU-20260004",
                    Sender = "Unilever Vietnam factory",
                    Receiver = "Unilever Frankfurt Hub",
                    CustomerId = customers[4].Id,
                    RouteId = routes[4].Id,
                    Weight = 18450.75m,
                    Status = "Delayed",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Shipment
                {
                    TrackingNo = "DMCO-VN-20260005",
                    Sender = "TSMC Taipei Lab",
                    Receiver = "Foxconn Hanoi Assembly",
                    CustomerId = customers[3].Id,
                    RouteId = routes[2].Id,
                    Weight = 320.25m,
                    Status = "InTransit",
                    CreatedAt = DateTime.UtcNow.AddHours(-18)
                }
            };

            context.Shipments.AddRange(shipments);
            context.SaveChanges();

            // 4. Seed RiskScores (Week 2 features)
            var riskScores = new[]
            {
                new RiskScore
                {
                    ShipmentId = shipments[0].Id,
                    Score = 15.5m,
                    RiskLevel = "Low",
                    Factors = "{\"CarrierPerformance\": 0.95, \"WeatherImpact\": 0.05, \"RouteCongestion\": 0.10}",
                    CalculatedAt = DateTime.UtcNow.AddHours(-10)
                },
                new RiskScore
                {
                    ShipmentId = shipments[1].Id,
                    Score = 48.2m,
                    RiskLevel = "Medium",
                    Factors = "{\"CarrierPerformance\": 0.72, \"WeatherImpact\": 0.40, \"RouteCongestion\": 0.35}",
                    CalculatedAt = DateTime.UtcNow
                },
                new RiskScore
                {
                    ShipmentId = shipments[3].Id,
                    Score = 85.9m,
                    RiskLevel = "High",
                    Factors = "{\"CarrierPerformance\": 0.35, \"WeatherImpact\": 0.92, \"RouteCongestion\": 0.88}",
                    CalculatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            context.RiskScores.AddRange(riskScores);
            context.SaveChanges();

            // 5. Seed ChatLogs (Week 2 features)
            var chatLogs = new[]
            {
                new ChatLog
                {
                    UserId = "user_01",
                    Prompt = "What is the custom clearance procedure for electronics shipped from SGN to SIN?",
                    Response = "Based on Dimerco standard operating procedures: 1. Ensure the Commercial Invoice lists accurate Harmonized System (HS) codes (usually Chapter 85 for electronics). 2. Submit the electronic export declaration through VNACCS/VCIS system. 3. Upon arrival in Singapore, clear cargo via TradeNet. Singapore imposes 9% GST unless importing under a temporary import scheme (e.g. Major Exporter Scheme).",
                    LLMProvider = "Claude",
                    Rating = 5,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new ChatLog
                {
                    UserId = "user_02",
                    Prompt = "Check status of shipment DMCO-EU-20260004.",
                    Response = "Shipment DMCO-EU-20260004 (Sender: Unilever VN, Receiver: Unilever FRA) is currently marked as **Delayed**. The latest transit update indicates route delays due to severe crosswind weather conditions at Noi Bai (HAN) terminal causing flight reschedules.",
                    LLMProvider = "Gemini",
                    Rating = 4,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            context.ChatLogs.AddRange(chatLogs);
            context.SaveChanges();

            // 6. Seed default Admin User for JWT authentication
            if (!context.Users.Any())
            {
                var adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            Console.WriteLine("Database Seeding Completed Successfully.");
        }

        /// <summary>
        /// Hashes a plain-text password using PBKDF2 with SHA256, 100,000 iterations.
        /// Format: {base64_salt}:{base64_hash}
        /// </summary>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a plain-text password against a stored PBKDF2 hash string.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] actualHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
    }
}
