using AdvancedOrderSystem.Data;
using AdvancedOrderSystem.Middleware;
using AdvancedOrderSystem.Repositories;
using AdvancedOrderSystem.Repositories.Impl;
using AdvancedOrderSystem.Services;
using AdvancedOrderSystem.Services.Impl;
using Microsoft.EntityFrameworkCore;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork
>();

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository
>();

builder.Services.AddScoped<
    ICustomerRepository,
    CustomerRepository
>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository
>();

builder.Services.AddScoped<
    IOrderService,
    OrderService
>();

builder.Services.AddScoped<
    IProductService,
    ProductService
>();

builder.Services.AddScoped<
    ICustomerService,
    CustomerService
>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS runs before the HTTPS redirect so preflight requests are not redirected
app.UseCors(FrontendCorsPolicy);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
