using Microsoft.EntityFrameworkCore;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Data
{
    public class SmartLogixDbContext : DbContext
    {
        public SmartLogixDbContext(DbContextOptions<SmartLogixDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<SmartLogix.WebApi.Models.Route> Routes => Set<SmartLogix.WebApi.Models.Route>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<RiskScore> RiskScores => Set<RiskScore>();
        public DbSet<ChatLog> ChatLogs => Set<ChatLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customers Table configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(255);
                entity.Property(c => c.Email).HasMaxLength(255);
                entity.Property(c => c.Phone).HasMaxLength(50);
            });

            // Routes Table configuration
            modelBuilder.Entity<SmartLogix.WebApi.Models.Route>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Source).IsRequired().HasMaxLength(255);
                entity.Property(r => r.Destination).IsRequired().HasMaxLength(255);
            });

            // Shipments Table configuration
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.TrackingNo).IsRequired().HasMaxLength(100);
                entity.HasIndex(s => s.TrackingNo).IsUnique(); // Enforce unique tracking numbers
                entity.Property(s => s.Sender).IsRequired().HasMaxLength(255);
                entity.Property(s => s.Receiver).IsRequired().HasMaxLength(255);
                entity.Property(s => s.Weight).HasPrecision(18, 2);
                entity.Property(s => s.Status).HasMaxLength(50);

                // Relationships
                entity.HasOne(s => s.Customer)
                      .WithMany(c => c.Shipments)
                      .HasForeignKey(s => s.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Route)
                      .WithMany(r => r.Shipments)
                      .HasForeignKey(s => s.RouteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // RiskScores Table configuration
            modelBuilder.Entity<RiskScore>(entity =>
            {
                entity.HasKey(rs => rs.Id);
                entity.Property(rs => rs.Score).HasPrecision(5, 2);
                entity.Property(rs => rs.RiskLevel).HasMaxLength(50);
                
                entity.HasOne(rs => rs.Shipment)
                      .WithMany(s => s.RiskScores)
                      .HasForeignKey(rs => rs.ShipmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ChatLogs Table configuration
            modelBuilder.Entity<ChatLog>(entity =>
            {
                entity.HasKey(cl => cl.Id);
                entity.Property(cl => cl.UserId).HasMaxLength(100);
                entity.Property(cl => cl.LLMProvider).HasMaxLength(50);
            });
        }
    }
}
