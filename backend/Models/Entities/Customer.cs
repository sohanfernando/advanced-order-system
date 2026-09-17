namespace AdvancedOrderSystem.Models.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    //One Customer can have Many Orders
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}