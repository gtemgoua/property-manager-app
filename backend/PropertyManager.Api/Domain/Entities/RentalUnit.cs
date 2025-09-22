using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Domain.Entities;

public class RentalUnit : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double SquareFeet { get; set; }
    public RentalUnitStatus Status { get; set; } = RentalUnitStatus.Available;
    public string? Notes { get; set; }

    public ICollection<RentalContract> Contracts { get; set; } = new List<RentalContract>();
}
