namespace HQ.Desktop.Models;

public sealed class HqBranchSeed
{
    public required string Name { get; init; }
    public double Weight { get; init; }
}

public sealed class HqCompanySeed
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string City { get; init; }
    public required IReadOnlyList<HqBranchSeed> Branches { get; init; }
}

public sealed class HqEntityMetric
{
    public required string Name { get; init; }
    public required string Subtitle { get; init; }
    public decimal Revenue { get; init; }
    public decimal PreviousRevenue { get; init; }
    public int Orders { get; init; }
    public int Guests { get; init; }
    public decimal Discounts { get; init; }
    public decimal Voids { get; init; }
    public decimal Cost { get; init; }
    public decimal DineIn { get; init; }
    public decimal Delivery { get; init; }
    public decimal Takeaway { get; init; }
    public decimal Cash { get; init; }
    public decimal Card { get; init; }
    public decimal Wallet { get; init; }
    public double[] Hourly { get; init; } = [];
    public double[] Trend { get; init; } = [];

    public decimal AverageTicket => Orders == 0 ? 0 : Revenue / Orders;
    public decimal Margin => Revenue == 0 ? 0 : (Revenue - Cost) / Revenue;
    public decimal Growth => PreviousRevenue == 0 ? 0 : (Revenue - PreviousRevenue) / PreviousRevenue;
    public decimal Profit => Revenue - Cost;
}

public sealed class HqDashboardSnapshot
{
    public required string ScopeTitle { get; init; }
    public required string ScopeHint { get; init; }
    public bool ComparingCompanies { get; init; }
    public int CompanyCount { get; init; }
    public int BranchCount { get; init; }
    public required IReadOnlyList<HqEntityMetric> Entities { get; init; }
    public required IReadOnlyList<string> TrendLabels { get; init; }
    public required IReadOnlyList<string> HourLabels { get; init; }
    public required IReadOnlyList<string> Insights { get; init; }

    public decimal Revenue => Entities.Sum(e => e.Revenue);
    public decimal PreviousRevenue => Entities.Sum(e => e.PreviousRevenue);
    public int Orders => Entities.Sum(e => e.Orders);
    public int Guests => Entities.Sum(e => e.Guests);
    public decimal Discounts => Entities.Sum(e => e.Discounts);
    public decimal Voids => Entities.Sum(e => e.Voids);
    public decimal Cost => Entities.Sum(e => e.Cost);
    public decimal Profit => Revenue - Cost;
    public decimal AverageTicket => Orders == 0 ? 0 : Revenue / Orders;
    public decimal Growth => PreviousRevenue == 0 ? 0 : (Revenue - PreviousRevenue) / PreviousRevenue;
    public decimal Margin => Revenue == 0 ? 0 : Profit / Revenue;
    public decimal DineIn => Entities.Sum(e => e.DineIn);
    public decimal Delivery => Entities.Sum(e => e.Delivery);
    public decimal Takeaway => Entities.Sum(e => e.Takeaway);
    public decimal Cash => Entities.Sum(e => e.Cash);
    public decimal Card => Entities.Sum(e => e.Card);
    public decimal Wallet => Entities.Sum(e => e.Wallet);
}
