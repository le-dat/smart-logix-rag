using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly SmartLogixDbContext _context;

        public ShipmentRepository(SmartLogixDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Shipment>> GetAllAsync()
        {
            return await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Route)
                .Include(s => s.RiskScores)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(int id)
        {
            return await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Route)
                .Include(s => s.RiskScores)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Shipment?> GetByTrackingNoAsync(string trackingNo)
        {
            return await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Route)
                .Include(s => s.RiskScores)
                .FirstOrDefaultAsync(s => s.TrackingNo == trackingNo);
        }

        public async Task<Shipment> CreateAsync(Shipment shipment)
        {
            await _context.Shipments.AddAsync(shipment);
            await _context.SaveChangesAsync();
            return shipment;
        }

        public async Task<bool> UpdateAsync(Shipment shipment)
        {
            _context.Shipments.Update(shipment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment == null)
            {
                return false;
            }
            _context.Shipments.Remove(shipment);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
