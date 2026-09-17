using AdvancedOrderSystem.Models.DTOs.Product;
using AdvancedOrderSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedOrderSystem.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAllProducts([FromQuery] string? search)
    {
        var products = await _productService.GetAllProductsAsync(search);

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(new
            {
                message = $"Product with id {id} was not found."
            });
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct(CreateProductRequest request)
    {
        // Validation errors (ArgumentException) become 400 in GlobalExceptionHandler
        var product = await _productService.CreateProductAsync(request);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = product.Id },
            product
        );
    }
}