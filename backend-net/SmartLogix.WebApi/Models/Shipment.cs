using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartLogix.WebApi.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        public string TrackingNo { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int RouteId { get; set; }
        public Route? Route { get; set; }

        public decimal Weight { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, InTransit, Delivered, Delayed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<RiskScore> RiskScores { get; set; } = new List<RiskScore>();
    }
}
