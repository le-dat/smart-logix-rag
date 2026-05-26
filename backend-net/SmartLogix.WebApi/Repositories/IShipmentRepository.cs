using System.Collections.Generic;
using System.Threading.Tasks;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Repositories
{
    public interface IShipmentRepository
    {
        Task<IEnumerable<Shipment>> GetAllAsync();
        Task<Shipment?> GetByIdAsync(int id);
        Task<Shipment?> GetByTrackingNoAsync(string trackingNo);
        Task<Shipment> CreateAsync(Shipment shipment);
        Task<bool> UpdateAsync(Shipment shipment);
        Task<bool> DeleteAsync(int id);
    }
}
