using System.Linq;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.DTOs
{
    public static class MappingExtensions
    {
        // Customer mappings
        public static CustomerDto ToDto(this Customer customer)
        {
            if (customer == null) return null!;
            return new CustomerDto(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone
            );
        }

        public static Customer ToEntity(this CustomerCreateDto dto)
        {
            if (dto == null) return null!;
            return new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone
            };
        }

        public static void UpdateEntity(this CustomerUpdateDto dto, Customer customer)
        {
            if (dto == null || customer == null) return;
            customer.Name = dto.Name;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
        }

        // Route mapping
        public static RouteDto ToDto(this SmartLogix.WebApi.Models.Route route)
        {
            if (route == null) return null!;
            return new RouteDto(
                route.Id,
                route.Source,
                route.Destination,
                route.AverageDuration
            );
        }

        // RiskScore mapping
        public static RiskScoreDto ToDto(this RiskScore riskScore)
        {
            if (riskScore == null) return null!;
            return new RiskScoreDto(
                riskScore.Id,
                riskScore.Score,
                riskScore.RiskLevel,
                riskScore.Factors,
                riskScore.CalculatedAt
            );
        }

        // Shipment mappings
        public static ShipmentDto ToDto(this Shipment shipment)
        {
            if (shipment == null) return null!;
            return new ShipmentDto(
                shipment.Id,
                shipment.TrackingNo,
                shipment.Sender,
                shipment.Receiver,
                shipment.CustomerId,
                shipment.Customer?.ToDto(),
                shipment.RouteId,
                shipment.Route?.ToDto(),
                shipment.Weight,
                shipment.Status,
                shipment.CreatedAt,
                shipment.RiskScores?.Select(r => r.ToDto()) ?? Enumerable.Empty<RiskScoreDto>()
            );
        }

        public static Shipment ToEntity(this ShipmentCreateDto dto)
        {
            if (dto == null) return null!;
            return new Shipment
            {
                TrackingNo = dto.TrackingNo,
                Sender = dto.Sender,
                Receiver = dto.Receiver,
                CustomerId = dto.CustomerId,
                RouteId = dto.RouteId,
                Weight = dto.Weight,
                Status = dto.Status
            };
        }

        public static void UpdateEntity(this ShipmentUpdateDto dto, Shipment shipment)
        {
            if (dto == null || shipment == null) return;
            shipment.TrackingNo = dto.TrackingNo;
            shipment.Sender = dto.Sender;
            shipment.Receiver = dto.Receiver;
            shipment.CustomerId = dto.CustomerId;
            shipment.RouteId = dto.RouteId;
            shipment.Weight = dto.Weight;
            shipment.Status = dto.Status;
        }
    }
}
