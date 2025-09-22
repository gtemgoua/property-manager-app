using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Contracts.Requests;

public record CreateRentalUnitRequest(
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
    string? Notes
);

public record UpdateRentalUnitRequest(
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
    string? Notes
);
