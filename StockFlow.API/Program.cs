using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using StockFlow.API.ExceptionHandling;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Application.Services;
using StockFlow.Application.Validators.CategoryValidators;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using StockFlow.Infrastructure;
using StockFlow.Infrastructure.Interceptors;
using StockFlow.Infrastructure.Repositories;
using StockFlow.Infrastructure.UOW;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<AuditDbContextInterceptor>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditDbContextInterceptor>());
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();


builder.Services.AddMapster();
builder.Services.AddValidatorsFromAssembly(typeof(CreateCategoryDtoValidator).Assembly);

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.Users.Any())
    {
        context.Users.Add(new User
        {
            Email = "admin@stockflow.com",
            PasswordHash = "gecici-hash",
            FullName = "Test Admin",
            EmployeeCode = "EMP-0001",
            Role = UserRole.Admin,
            IsActive = true
        });
        context.SaveChanges();
    }
}

app.Run();