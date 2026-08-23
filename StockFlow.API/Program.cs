using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Infrastructure;
using StockFlow.Infrastructure.Repositories;
using StockFlow.Infrastructure.UOW;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();


builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
