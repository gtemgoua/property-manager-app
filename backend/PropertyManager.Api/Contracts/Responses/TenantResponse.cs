namespace PropertyManager.Api.Contracts.Responses;

public record TenantResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes,
    DateTime CreatedAt
);
