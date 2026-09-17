namespace AdvancedOrderSystem.Models.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    // Many OrderItems belong to one Order
    public Order Order { get; set; } = null!;

    // Many OrderItems can reference one Product
    public Product Product { get; set; } = null!;
}