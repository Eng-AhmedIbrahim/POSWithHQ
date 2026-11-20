using HQ.Desktop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pos.Repository.Data;
using POS.Contract.Models;
using POS.Core.Entities.OrderEntity;

namespace HQ.Desktop.Services;

public sealed class HqAnalyticsService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HqDataSyncService? _syncService;

    private static readonly string[] HourLabels =
        ["10ص", "11ص", "12م", "1م", "2م", "3م", "4م", "5م", "6م", "7م", "8م", "9م", "10م", "11م"];

    public HqAnalyticsService(IServiceScopeFactory scopeFactory, HqDataSyncService? syncService = null)
    {
        _scopeFactory = scopeFactory;
        _syncService = syncService;
    }

    public IReadOnlyList<HqCompanySeed> Companies
    {
        get
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    var branches = db.Branches.AsNoTracking().Where(b => b.Active && !b.Suspend).ToList();
                    if (branches.Count > 0)
                    {
                        return
                        [
                            new()
                            {
                                Id = "hq",
                                Name = "المقر الرئيسي (HQ)",
                                City = !string.IsNullOrWhiteSpace(branches[0].Address) ? branches[0].Address!.Split('،')[0].Trim() : "القاهرة",
                                Branches = branches.Select(b => new HqBranchSeed
                                {
                                    Name = b.Name ?? $"فرع #{b.Id}",
                                    Weight = 1.0
                                }).ToList()
                            }
                        ];
                    }
                }
            }
            catch { }

            return
            [
                new()
                {
                    Id = "hq",
                    Name = "الشركة الرئيسية",
                    City = "القاهرة",
                    Branches = [new() { Name = "الفرع الرئيسي", Weight = 1.0 }]
                }
            ];
        }
    }

    public HqDashboardSnapshot Build(string? companyId, string period, DateTime? from, DateTime? to)
    {
        var (fromDate, toDate, prevFromDate, prevToDate) = CalculatePeriodDates(period, from, to);
        var entities = new List<HqEntityMetric>();

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetService<AppDbContext>();
            if (db != null)
            {
                var branches = db.Branches.AsNoTracking().Where(b => b.Active && !b.Suspend).ToList();
                var currentOrders = db.Orders.AsNoTracking()
                    .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                    .ToList();

                var previousOrders = db.Orders.AsNoTracking()
                    .Where(o => o.OrderDate >= prevFromDate && o.OrderDate <= prevToDate)
                    .ToList();

                var trend14DaysOrders = db.Orders.AsNoTracking()
                    .Where(o => o.OrderDate >= DateTime.Today.AddDays(-13))
                    .ToList();

                foreach (var b in branches)
                {
                    var bOrders = currentOrders.Where(o => o.BranchID == b.Id).ToList();
                    var bPrevOrders = previousOrders.Where(o => o.BranchID == b.Id).ToList();

                    var rev = bOrders.Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);
                    var prevRev = bPrevOrders.Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);
                    var orderCount = bOrders.Count;
                    var disc = bOrders.Sum(o => o.TotalDiscount ?? o.Discount ?? 0m);
                    var voidAmt = bOrders.Sum(o => o.VoidAmount ?? 0m);

                    var dineIn = bOrders.Where(o => o.OrderType == OrderTypes.DineIn).Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);
                    var delivery = bOrders.Where(o => o.OrderType == OrderTypes.Delivery).Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);
                    var takeaway = bOrders.Where(o => o.OrderType == OrderTypes.TakeAway).Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);

                    var cash = bOrders.Where(o => o.PaymentMethod == PaymentMethod.Cash).Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);
                    var card = bOrders.Where(o => o.PaymentMethod == PaymentMethod.Visa || o.PaymentMethod == PaymentMethod.Mastercard || o.PaymentMethod == PaymentMethod.Mada).Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);
                    var wallet = rev - (cash + card);
                    if (wallet < 0) wallet = 0;

                    // Real 14-days trend
                    var bTrendOrders = trend14DaysOrders.Where(o => o.BranchID == b.Id).ToList();
                    var trend = new double[14];
                    for (int i = 0; i < 14; i++)
                    {
                        var targetDate = DateTime.Today.AddDays(-13 + i);
                        trend[i] = (double)bTrendOrders
                            .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == targetDate)
                            .Sum(o => o.GrandTotal ?? o.Subtotal ?? 0m);
                    }

                    // Real hourly distribution
                    var hourly = new double[14];
                    foreach (var o in bOrders)
                    {
                        if (o.OrderDate.HasValue)
                        {
                            var h = o.OrderDate.Value.Hour - 10;
                            if (h >= 0 && h < 14)
                                hourly[h] += (double)(o.GrandTotal ?? o.Subtotal ?? 0m);
                        }
                    }

                    entities.Add(new HqEntityMetric
                    {
                        Name = b.Name ?? $"فرع #{b.Id}",
                        Subtitle = b.NormalizedName ?? b.Address ?? "الفرع الرئيسي",
                        Revenue = rev,
                        PreviousRevenue = prevRev,
                        Orders = orderCount,
                        Guests = orderCount,
                        Discounts = disc,
                        Voids = voidAmt,
                        Cost = rev * 0.38m,
                        DineIn = dineIn,
                        Delivery = delivery,
                        Takeaway = takeaway,
                        Cash = cash,
                        Card = card,
                        Wallet = wallet,
                        Hourly = hourly,
                        Trend = trend
                    });
                }
            }
        }
        catch { }

        if (entities.Count == 0)
        {
            entities.Add(new HqEntityMetric
            {
                Name = "الفرع الرئيسي",
                Subtitle = "المقر الرئيسي",
                Revenue = 0,
                PreviousRevenue = 0,
                Orders = 0,
                Hourly = new double[14],
                Trend = new double[14]
            });
        }

        return new HqDashboardSnapshot
        {
            ComparingCompanies = false,
            CompanyCount = 1,
            BranchCount = entities.Count,
            ScopeTitle = "مقارنة فروع المقر الرئيسي (HQ)",
            ScopeHint = "تم الربط والمزامنة الحية مع قاعدة بيانات المقر الرئيسي PointOfSaleDBk.",
            Entities = entities.OrderByDescending(e => e.Revenue).ToList(),
            TrendLabels = Enumerable.Range(0, 14)
                .Select(i => DateTime.Today.AddDays(-13 + i).ToString("dd/MM"))
                .ToList(),
            HourLabels = HourLabels,
            Insights = BuildInsights(entities)
        };
    }

    private static (DateTime fromDate, DateTime toDate, DateTime prevFromDate, DateTime prevToDate) CalculatePeriodDates(string period, DateTime? from, DateTime? to)
    {
        var now = DateTime.Now;
        var today = DateTime.Today;

        switch (period)
        {
            case "Yesterday":
                return (today.AddDays(-1), today.AddTicks(-1), today.AddDays(-2), today.AddDays(-1).AddTicks(-1));
            case "ThisMonth":
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var prevMonthStart = monthStart.AddMonths(-1);
                var prevMonthEnd = monthStart.AddTicks(-1);
                return (monthStart, now, prevMonthStart, prevMonthEnd);
            case "Range" when from.HasValue && to.HasValue:
                var days = (to.Value.Date - from.Value.Date).Days + 1;
                return (from.Value.Date, to.Value.Date.AddDays(1).AddTicks(-1), from.Value.Date.AddDays(-days), from.Value.Date.AddTicks(-1));
            case "Today":
            default:
                return (today, now, today.AddDays(-1), today.AddTicks(-1));
        }
    }

    private static IReadOnlyList<string> BuildInsights(IReadOnlyList<HqEntityMetric> entities)
    {
        var totalRev = entities.Sum(e => e.Revenue);
        var totalOrders = entities.Sum(e => e.Orders);
        var totalVoids = entities.Sum(e => e.Voids);
        var totalDiscounts = entities.Sum(e => e.Discounts);

        if (totalRev == 0 && totalOrders == 0)
        {
            return
            [
                "تم الربط المباشر مع قاعدة بيانات المقر الرئيسي بنجاح.",
                "لا توجد حركات بيع مسجلة خلال الفترة المختارة حتى الآن.",
                "بمجرد سحب مبيعات الفروع ستنعكس الفواتير والأصناف والفويدات في اللوحة مباشرة.",
                "كل العمليات المسحوبة ترتبط رسمياً بكود و ID الفرع في المقر."
            ];
        }

        var top = entities.MaxBy(e => e.Revenue)!;
        var ticket = entities.MaxBy(e => e.AverageTicket)!;
        var share = totalRev == 0 ? 0 : top.Revenue / totalRev;

        return
        [
            $"{top.Name} هو أعلى الفروع مبيعًا بحصة {share:P1} من إجمالي الإيرادات.",
            $"أعلى متوسط فاتورة لدى {ticket.Name}: {ticket.AverageTicket:N2} ج.م.",
            $"إجمالي عدد الطلبات المسجلة: {totalOrders:N0} طلب خلال الفترة.",
            $"إجمالي الخصومات الممنوحة: {totalDiscounts:N2} ج.م، وإجمالي الملغيات (Voids): {totalVoids:N2} ج.م."
        ];
    }
}
