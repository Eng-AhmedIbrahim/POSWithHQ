using HQ.Desktop.Models;

namespace HQ.Desktop.Services;

public sealed class HqAnalyticsService
{
    private readonly HqDataSyncService? _syncService;

    public HqAnalyticsService(HqDataSyncService? syncService = null)
    {
        _syncService = syncService;
    }
    private static readonly string[] HourLabels =
        ["10ص", "11ص", "12م", "1م", "2م", "3م", "4م", "5م", "6م", "7م", "8م", "9م", "10م", "11م"];

    public IReadOnlyList<HqCompanySeed> Companies { get; } =
    [
        new()
        {
            Id = "nt",
            Name = "نيو تك",
            City = "القاهرة",
            Branches =
            [
                new() { Name = "مدينة نصر", Weight = 1.18 },
                new() { Name = "المعادي", Weight = 0.96 },
                new() { Name = "التجمع", Weight = 1.32 },
                new() { Name = "الزمالك", Weight = 0.84 }
            ]
        },
        new()
        {
            Id = "gold",
            Name = "جولد كيتشن",
            City = "الجيزة",
            Branches =
            [
                new() { Name = "المهندسين", Weight = 1.05 },
                new() { Name = "الدقي", Weight = 0.78 },
                new() { Name = "الشيخ زايد", Weight = 1.22 }
            ]
        },
        new()
        {
            Id = "coast",
            Name = "كوست هاوس",
            City = "الإسكندرية",
            Branches =
            [
                new() { Name = "ستانلي", Weight = 1.41 },
                new() { Name = "سموحة", Weight = 0.91 },
                new() { Name = "العجمي", Weight = 0.73 }
            ]
        }
    ];

    public HqDashboardSnapshot Build(string? companyId, string period, DateTime? from, DateTime? to)
    {
        var days = PeriodDays(period, from, to);
        var factor = PeriodFactor(period, days);
        var selected = string.IsNullOrWhiteSpace(companyId)
            ? null
            : Companies.FirstOrDefault(c => c.Id == companyId);

        var comparingCompanies = selected is null && Companies.Count > 1;
        IReadOnlyList<HqEntityMetric> entities = comparingCompanies
            ? Companies.Select(c => AggregateCompany(c, factor, days)).ToList()
            : (selected ?? Companies[0]).Branches.Select(b => BuildBranch(selected ?? Companies[0], b, factor, days)).ToList();

        var scopeCompany = selected ?? (Companies.Count == 1 ? Companies[0] : null);
        return new HqDashboardSnapshot
        {
            ComparingCompanies = comparingCompanies,
            CompanyCount = Companies.Count,
            BranchCount = scopeCompany?.Branches.Count ?? Companies.Sum(c => c.Branches.Count),
            ScopeTitle = comparingCompanies
                ? "مقارنة بين الشركات"
                : $"مقارنة فروع {scopeCompany!.Name}",
            ScopeHint = comparingCompanies
                ? "المقر مرتبط بأكثر من شركة، فالمقارنة هنا على مستوى الشركات. اختَر شركة لفتح مقارنة فروعها."
                : "شركة واحدة داخل النطاق الحالي، فالمقارنة هنا بين فروعها.",
            Entities = entities.OrderByDescending(e => e.Revenue).ToList(),
            TrendLabels = Enumerable.Range(0, 14)
                .Select(i => DateTime.Today.AddDays(-13 + i).ToString("dd/MM"))
                .ToList(),
            HourLabels = HourLabels,
            Insights = BuildInsights(entities, comparingCompanies)
        };
    }

    private static HqEntityMetric AggregateCompany(HqCompanySeed company, double factor, int days)
    {
        var branches = company.Branches
            .Select(b => BuildBranch(company, b, factor, days))
            .ToList();

        return new HqEntityMetric
        {
            Name = company.Name,
            Subtitle = $"{company.City} · {company.Branches.Count} فروع",
            Revenue = branches.Sum(b => b.Revenue),
            PreviousRevenue = branches.Sum(b => b.PreviousRevenue),
            Orders = branches.Sum(b => b.Orders),
            Guests = branches.Sum(b => b.Guests),
            Discounts = branches.Sum(b => b.Discounts),
            Voids = branches.Sum(b => b.Voids),
            Cost = branches.Sum(b => b.Cost),
            DineIn = branches.Sum(b => b.DineIn),
            Delivery = branches.Sum(b => b.Delivery),
            Takeaway = branches.Sum(b => b.Takeaway),
            Cash = branches.Sum(b => b.Cash),
            Card = branches.Sum(b => b.Card),
            Wallet = branches.Sum(b => b.Wallet),
            Hourly = SumSeries(branches.Select(b => b.Hourly)),
            Trend = SumSeries(branches.Select(b => b.Trend))
        };
    }

    private static HqEntityMetric BuildBranch(HqCompanySeed company, HqBranchSeed branch, double factor, int days)
    {
        var seed = Hash($"{company.Id}:{branch.Name}");
        var baseDaily = 42000m * (decimal)branch.Weight;
        var revenue = Round(baseDaily * (decimal)factor * (decimal)Wave(seed, 1.0));
        var previous = Round(revenue * (decimal)(0.86 + ((seed % 17) / 100.0)));
        var orders = (int)Math.Round((double)revenue / (185 + (seed % 40)));
        var dineShare = 0.46 + ((seed % 9) / 100.0);
        var deliveryShare = 0.28 + ((seed % 7) / 100.0);
        var takeawayShare = Math.Max(0.08, 1 - dineShare - deliveryShare);
        var cashShare = 0.38 + ((seed % 11) / 200.0);
        var cardShare = 0.34 + ((seed % 5) / 100.0);
        var walletShare = Math.Max(0.08, 1 - cashShare - cardShare);

        return new HqEntityMetric
        {
            Name = branch.Name,
            Subtitle = company.Name,
            Revenue = revenue,
            PreviousRevenue = previous,
            Orders = Math.Max(orders, 1),
            Guests = (int)Math.Round(orders * (1.6 + (seed % 8) / 20.0)),
            Discounts = Round(revenue * (decimal)(0.035 + (seed % 6) / 400.0)),
            Voids = Round(revenue * (decimal)(0.008 + (seed % 4) / 800.0)),
            Cost = Round(revenue * (decimal)(0.41 + (seed % 10) / 200.0)),
            DineIn = Round(revenue * (decimal)dineShare),
            Delivery = Round(revenue * (decimal)deliveryShare),
            Takeaway = Round(revenue * (decimal)takeawayShare),
            Cash = Round(revenue * (decimal)cashShare),
            Card = Round(revenue * (decimal)cardShare),
            Wallet = Round(revenue * (decimal)walletShare),
            Hourly = HourCurve(seed, revenue),
            Trend = TrendCurve(seed, revenue, days)
        };
    }

    private static IReadOnlyList<string> BuildInsights(IReadOnlyList<HqEntityMetric> entities, bool companies)
    {
        if (entities.Count == 0)
            return ["لا توجد وحدات للمقارنة داخل هذا النطاق."];

        var label = companies ? "شركة" : "فرع";
        var top = entities.MaxBy(e => e.Revenue)!;
        var grower = entities.MaxBy(e => e.Growth)!;
        var weakestMargin = entities.MinBy(e => e.Margin)!;
        var ticket = entities.MaxBy(e => e.AverageTicket)!;
        var total = entities.Sum(e => e.Revenue);
        var share = total == 0 ? 0 : top.Revenue / total;

        return
        [
            $"{top.Name} أعلى {label} مبيعًا بحصة {share:P1} من إجمالي النطاق.",
            $"{grower.Name} الأسرع نموًا مقابل الفترة السابقة ({grower.Growth:P1}).",
            $"أعلى متوسط فاتورة عند {ticket.Name}: {ticket.AverageTicket:N0} ج.م.",
            $"أضعف هامش ربح عند {weakestMargin.Name}: {weakestMargin.Margin:P1}.",
            $"إجمالي الخصومات {entities.Sum(e => e.Discounts):N0} ج.م، والملغيات {entities.Sum(e => e.Voids):N0} ج.م."
        ];
    }

    private static int PeriodDays(string period, DateTime? from, DateTime? to) => period switch
    {
        "Yesterday" => 1,
        "ThisMonth" => DateTime.Today.Day,
        "Range" when from.HasValue && to.HasValue => Math.Max(1, (to.Value.Date - from.Value.Date).Days + 1),
        _ => 1
    };

    private static double PeriodFactor(string period, int days) => period switch
    {
        "Yesterday" => 0.93,
        "ThisMonth" => Math.Max(days, 1) * 0.97,
        "Range" => Math.Max(days, 1) * 0.95,
        _ => 1
    };

    private static double[] HourCurve(int seed, decimal revenue)
    {
        double[] shape = [0.3, 0.45, 0.7, 1.15, 1.35, 0.85, 0.55, 0.7, 1.05, 1.45, 1.25, 0.9, 0.55, 0.3];
        var shift = seed % 3;
        var rotated = shape.Select((_, i) => shape[(i + shift) % shape.Length]).ToArray();
        var sum = rotated.Sum();
        return rotated.Select(v => Math.Round((double)revenue * v / sum, 0)).ToArray();
    }

    private static double[] TrendCurve(int seed, decimal revenue, int days)
    {
        var daily = (double)revenue / Math.Max(days, 1);
        return Enumerable.Range(0, 14).Select(i =>
        {
            var wave = 0.82 + 0.22 * Math.Sin((i + seed % 5) * 0.7);
            var weekend = i % 7 is 4 or 5 ? 1.16 : 1;
            return Math.Round(daily * wave * weekend, 0);
        }).ToArray();
    }

    private static double[] SumSeries(IEnumerable<double[]> series)
    {
        var list = series.ToList();
        if (list.Count == 0)
            return [];
        var length = list[0].Length;
        return Enumerable.Range(0, length)
            .Select(i => Math.Round(list.Sum(s => s[i]), 0))
            .ToArray();
    }

    private static decimal Round(decimal value) => Math.Round(value, 0);
    private static double Wave(int seed, double scale) => scale * (0.94 + (seed % 13) / 100.0);
    private static int Hash(string value) => Math.Abs(value.Aggregate(17, (current, ch) => current * 31 + ch));
}
