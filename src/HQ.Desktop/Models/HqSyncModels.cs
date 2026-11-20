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

public sealed class HqOrderAuditDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = "الفرع الرئيسي";
    public DateTime OrderDate { get; set; }
    public string OrderType { get; set; } = "صالة";
    public string OrderState { get; set; } = "مكتمل";
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Service { get; set; }
    public decimal DeliveryFees { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal Paid { get; set; }
    public decimal Remain { get; set; }
    public string? CashierName { get; set; }
    public string? WaiterName { get; set; }
    public string? DriverName { get; set; }
    public string? CustomerName { get; set; }
    public string? Phone { get; set; }
    public string? TableName { get; set; }
    public string? PaymentMethod { get; set; }
    public bool HasVoid { get; set; }
    public decimal VoidAmount { get; set; }
    public string? VoidByName { get; set; }
    public string? VoidReason { get; set; }
    public DateTime? VoidTime { get; set; }
    public string? DiscountReason { get; set; }
    public string? DiscountByName { get; set; }
    public bool IsHospitality { get; set; }
    public bool IsStaffMeal { get; set; }
    public string? OrderNotice { get; set; }
    public List<HqOrderItemAuditDto> Items { get; set; } = new();
}

public sealed class HqOrderItemAuditDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public bool IsVoided { get; set; }
    public decimal VoidQuantity { get; set; }
    public decimal TotalVoidAmount { get; set; }
    public string? VoidByName { get; set; }
    public string? VoidReason { get; set; }
    public DateTime? VoidTime { get; set; }
    public string? LineComment { get; set; }
    public List<HqOrderItemAttributeDto> Attributes { get; set; } = new();
}

public sealed class HqOrderItemAttributeDto
{
    public string AttributeName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
}

public sealed class HqDailyPnLDto
{
    public DateTime Date { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalGrossSales { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalVoids { get; set; }
    public decimal NetSales { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal TotalWasteCost { get; set; }
    public decimal TotalInventoryPurchases { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public int OrderCount { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public sealed class HqCostBatchDto
{
    public int BranchId { get; set; }
    public int MenuSalesItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchSource { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? UnitName { get; set; }
    public int Priority { get; set; }
    public bool IsExhausted { get; set; }
    public DateTime ReceivedAt { get; set; }
}

public sealed class HqSyncMetricsReport
{
    public int TotalOrdersSynced { get; set; }
    public int TotalOrderItemsSynced { get; set; }
    public int TotalAttributesSynced { get; set; }
    public int TotalVoidsSynced { get; set; }
    public int TotalRequisitionsSynced { get; set; }
    public int TotalPnLSnapshotsSynced { get; set; }
    public int TotalCostBatchesSynced { get; set; }
    public int TotalExpensesSynced { get; set; }
    public decimal TotalSalesValueSynced { get; set; }
    public decimal TotalVoidValueSynced { get; set; }
    public DateTime LastSyncTimestamp { get; set; } = DateTime.Now;
}

public sealed class HqVoidAuditEntry
{
    public int OrderId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Scope { get; set; } = "فاتورة كاملة"; // "فاتورة كاملة" or "صنف فردي"
    public string ItemName { get; set; } = "كامل الفاتورة";
    public decimal Quantity { get; set; } = 1;
    public decimal Amount { get; set; }
    public string VoidByName { get; set; } = string.Empty;
    public string VoidReason { get; set; } = string.Empty;
    public DateTime VoidTime { get; set; }
    public string TableOrCustomer { get; set; } = string.Empty;
}

public sealed class HqWarehouseStockItemDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UnitName { get; set; } = "كجم";
    public decimal QuantityOnHand { get; set; }
    public decimal WeightedAverageCost { get; set; }
    public decimal TotalValuation => QuantityOnHand * WeightedAverageCost;
    public decimal MinStockLevel { get; set; }
    public string Status => QuantityOnHand <= 0 ? "نفد" : (QuantityOnHand <= MinStockLevel ? "منخفض" : "متوفر");
}

public sealed class HqRequisitionDetailDto
{
    public int RequisitionId { get; set; }
    public string RequisitionNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string TargetWarehouseName { get; set; } = "المخزن الرئيسي المركزي";
    public DateTime RequestedDate { get; set; }
    public string Status { get; set; } = "قيد المراجعة";
    public decimal EstimatedTotal { get; set; }
    public List<HqRequisitionLineDto> Lines { get; set; } = new();
}

public sealed class HqRequisitionLineDto
{
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = "قطعة";
    public decimal RequestedQty { get; set; }
    public decimal ApprovedQty { get; set; }
    public decimal DispatchedQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal => RequestedQty * UnitCost;
}

public sealed class HqStockMovementDto
{
    public int Id { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string MovementType { get; set; } = "توريد مورد"; // "توريد مورد", "صرف لفرع", "هالك وتالف", "تسوية جردية"
    public decimal Quantity { get; set; }
    public string UnitName { get; set; } = "كجم";
    public decimal UnitCost { get; set; }
    public decimal TotalCost => Quantity * UnitCost;
    public DateTime MovementDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
