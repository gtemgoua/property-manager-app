using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Contracts.Responses;

public record RentalUnitResponse(
    Guid Id,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    decimal MonthlyRent,
    int Bedrooms,
    int Bathrooms,
    double SquareFeet,
    RentalUnitStatus Status,
    string? Notes,
    DateTime CreatedAt
);
