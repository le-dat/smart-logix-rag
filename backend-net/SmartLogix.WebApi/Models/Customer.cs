using System.Text.Json.Serialization;

namespace SmartLogix.WebApi.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
