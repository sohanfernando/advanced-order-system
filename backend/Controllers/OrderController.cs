using AdvancedOrderSystem.Models.DTOs.Common;
using AdvancedOrderSystem.Models.DTOs.Order;
using AdvancedOrderSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedOrderSystem.Controllers;

// ArgumentException -> 400 and KeyNotFoundException -> 404 are handled by GlobalExceptionHandler
[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(
        IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ==================================
    // Task 5 - Create Order
    // ==================================

    [HttpPost]
    public async Task<ActionResult<OrderResponse>>
        CreateOrder(
            CreateOrderRequest request)
    {
        var order =
            await _orderService
                .CreateOrderAsync(request);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = order.Id },
            order
        );
    }

    // ==================================
    // Task 10 - Get order
    // ==================================

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>>
        GetOrderById(int id)
    {
        var order =
            await _orderService
                .GetOrderByIdAsync(id);

        if (order == null)
        {
            return NotFound(new
            {
                message =
                    $"Order with id {id} was not found."
            });
        }

        return Ok(order);
    }

    // ==================================
    // Task 11 - Get all / filtering
    // ==================================

    [HttpGet]
    public async Task<
        ActionResult<
            PagedResponse<OrderListResponse>>>
        GetOrders(
            [FromQuery] string? status,
            [FromQuery] int? customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
    {
        var result =
            await _orderService
                .GetOrdersAsync(
                    status,
                    customerId,
                    page,
                    pageSize
                );

        return Ok(result);
    }

    // ==================================
    // Task 12 - Cancel
    // ==================================

    [HttpPatch("{id}/cancel")]
    public async Task<ActionResult<OrderResponse>>
        CancelOrder(int id)
    {
        var order =
            await _orderService
                .CancelOrderAsync(id);

        return Ok(order);
    }
}
