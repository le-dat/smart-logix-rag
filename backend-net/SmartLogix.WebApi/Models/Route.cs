using System.Text.Json.Serialization;

namespace SmartLogix.WebApi.Models
{
    public class Route
    {
        public int Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int AverageDuration { get; set; } // in hours

        [JsonIgnore]
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
