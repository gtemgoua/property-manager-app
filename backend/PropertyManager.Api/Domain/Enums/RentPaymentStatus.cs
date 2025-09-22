namespace PropertyManager.Api.Domain.Enums;

public enum RentPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Partial = 2,
    Late = 3,
    Waived = 4
}

public enum Currency
{
    USD = 0,
    EUR = 1,
    XAF = 2
}
