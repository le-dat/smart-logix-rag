using System.Collections.Generic;
using System.Threading.Tasks;
using SmartLogix.WebApi.Models;
using SmartLogix.WebApi.Repositories;

namespace SmartLogix.WebApi.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepository;

        public ShipmentService(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<IEnumerable<Shipment>> GetAllShipmentsAsync()
        {
            return await _shipmentRepository.GetAllAsync();
        }

        public async Task<Shipment?> GetShipmentByIdAsync(int id)
        {
            return await _shipmentRepository.GetByIdAsync(id);
        }

        public async Task<Shipment?> GetShipmentByTrackingNoAsync(string trackingNo)
        {
            return await _shipmentRepository.GetByTrackingNoAsync(trackingNo);
        }

        public async Task<Shipment> CreateShipmentAsync(Shipment shipment)
        {
            return await _shipmentRepository.CreateAsync(shipment);
        }

        public async Task<bool> UpdateShipmentAsync(Shipment shipment)
        {
            return await _shipmentRepository.UpdateAsync(shipment);
        }

        public async Task<bool> DeleteShipmentAsync(int id)
        {
            return await _shipmentRepository.DeleteAsync(id);
        }
    }
}
