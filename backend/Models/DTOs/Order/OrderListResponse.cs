namespace AdvancedOrderSystem.Models.DTOs.Order;

public class OrderListResponse
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;
}