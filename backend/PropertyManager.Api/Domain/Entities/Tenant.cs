namespace PropertyManager.Api.Domain.Entities;

public class Tenant : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Notes { get; set; }

    public ICollection<RentalContract> Contracts { get; set; } = new List<RentalContract>();
}
