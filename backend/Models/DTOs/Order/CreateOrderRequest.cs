namespace AdvancedOrderSystem.Models.DTOs.Order;

public class CreateOrderRequest
{
    public int CustomerId { get; set; }

    public decimal DiscountPercent { get; set; }

    public List<CreateOrderItemRequest> Items { get; set; } = new();
}