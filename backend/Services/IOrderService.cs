using AdvancedOrderSystem.Models.DTOs.Common;
using AdvancedOrderSystem.Models.DTOs.Order;

namespace AdvancedOrderSystem.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(
        CreateOrderRequest request);

    Task<OrderResponse?> GetOrderByIdAsync(
        int id);

    Task<PagedResponse<OrderListResponse>>
        GetOrdersAsync(
            string? status,
            int? customerId,
            int page,
            int pageSize);

    Task<OrderResponse> CancelOrderAsync(
        int id);
}