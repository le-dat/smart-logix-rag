using System;
using System.Text.Json.Serialization;

namespace SmartLogix.WebApi.Models
{
    public class RiskScore
    {
        public int Id { get; set; }
        
        public int ShipmentId { get; set; }
        
        [JsonIgnore]
        public Shipment? Shipment { get; set; }

        public decimal Score { get; set; } // 0.0 to 100.0 or 0.0 to 1.0
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
        public string Factors { get; set; } = "{}"; // JSON string listing key contributing risk factors
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}
