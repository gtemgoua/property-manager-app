namespace PropertyManager.Api.Contracts.Requests;

public record CreateTenantRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes
);

public record UpdateTenantRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes
);
