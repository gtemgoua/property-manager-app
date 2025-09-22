namespace PropertyManager.Api.Contracts.Responses;

public record RentCollectionChartPoint(
    int Year,
    int Month,
    decimal AmountDue,
    decimal AmountPaid
);

public record OccupancyChartPoint(
    int Year,
    int Month,
    int OccupiedUnits,
    int VacantUnits
);

public record DashboardMetricsResponse(
    int TotalTenants,
    int TotalUnits,
    int OccupiedUnits,
    int VacantUnits,
    decimal MonthlyRecurringRevenue,
    decimal MonthlyCollected,
    decimal MonthlyOutstanding,
    IEnumerable<RentCollectionChartPoint> RentCollection,
    IEnumerable<OccupancyChartPoint> Occupancy
);
