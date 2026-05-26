using System;
using System.Collections.Generic;

namespace SmartLogix.WebApi.DTOs
{
    public record RouteDto(
        int Id,
        string Source,
        string Destination,
        int AverageDuration
    );

    public record RiskScoreDto(
        int Id,
        decimal Score,
        string RiskLevel,
        string Factors,
        DateTime CalculatedAt
    );

    public record ShipmentDto(
        int Id,
        string TrackingNo,
        string Sender,
        string Receiver,
        int CustomerId,
        CustomerDto? Customer,
        int RouteId,
        RouteDto? Route,
        decimal Weight,
        string Status,
        DateTime CreatedAt,
        IEnumerable<RiskScoreDto> RiskScores
    );

    public record ShipmentCreateDto(
        string TrackingNo,
        string Sender,
        string Receiver,
        int CustomerId,
        int RouteId,
        decimal Weight,
        string Status
    );

    public record ShipmentUpdateDto(
        int Id,
        string TrackingNo,
        string Sender,
        string Receiver,
        int CustomerId,
        int RouteId,
        decimal Weight,
        string Status
    );
}
