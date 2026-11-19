using System.Text.Json.Serialization;

namespace HQ.Desktop.Models;

public enum HqNodeType
{
    Branch = 1,
    MainWarehouse = 2
}

public enum HqSyncStatus
{
    Online = 1,
    Offline = 2,
    Syncing = 3,
    Error = 4,
    Pending = 5
}

public sealed class HqNodeConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public HqNodeType NodeType { get; set; } = HqNodeType.Branch;
    public string City { get; set; } = "القاهرة";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ManagerName { get; set; }
    public string ApiUrl { get; set; } = "http://localhost:5050";
    public string? ApiKey { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Linked Warehouse (for branches)
    public string? LinkedWarehouseId { get; set; }
    public string? LinkedWarehouseName { get; set; }

    // Sync metrics
    public DateTime? LastSyncAt { get; set; }
    public HqSyncStatus Status { get; set; } = HqSyncStatus.Pending;
    public string? LastSyncMessage { get; set; }
    public long? PingLatencyMs { get; set; }
    public int PulledRecordsCount { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodayOrdersCount { get; set; }
    public decimal StockValuation { get; set; }
    public int StockItemsCount { get; set; }
    public int LowStockItemsCount { get; set; }
}

public sealed class HqSyncLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public HqNodeType NodeType { get; set; }
    public string Action { get; set; } = "سحب بيانات";
    public bool Success { get; set; }
    public long DurationMs { get; set; }
    public int RecordsCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class HqCentralWarehouseSummary
{
    public string WarehouseId { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public long PingMs { get; set; }
    public decimal TotalValuation { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int PendingRequisitionsCount { get; set; }
    public DateTime? LastSyncTime { get; set; }
    public List<HqLowStockItem> LowStockAlerts { get; set; } = new();
    public List<HqBranchRequisitionSummary> PendingRequisitions { get; set; } = new();
}

public sealed class HqLowStockItem
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal MinReorderPoint { get; set; }
    public string Unit { get; set; } = "قطعة";
    public decimal CostPrice { get; set; }
}

public sealed class HqBranchRequisitionSummary
{
    public string RequisitionNumber { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public int ItemsCount { get; set; }
    public string Status { get; set; } = "قيد الانتظار";
    public decimal EstimatedTotal { get; set; }
}

public sealed class HqTopProductMetric
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public string TopSellingBranch { get; set; } = string.Empty;
    public double GrowthPercentage { get; set; }
}
