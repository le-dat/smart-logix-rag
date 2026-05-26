using System.Collections.Generic;
using System.Threading.Tasks;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Services
{
    public interface IShipmentService
    {
        Task<IEnumerable<Shipment>> GetAllShipmentsAsync();
        Task<Shipment?> GetShipmentByIdAsync(int id);
        Task<Shipment?> GetShipmentByTrackingNoAsync(string trackingNo);
        Task<Shipment> CreateShipmentAsync(Shipment shipment);
        Task<bool> UpdateShipmentAsync(Shipment shipment);
        Task<bool> DeleteShipmentAsync(int id);
    }
}
