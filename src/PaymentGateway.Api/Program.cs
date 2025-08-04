using Microsoft.EntityFrameworkCore;

using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Services;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IBankService, PaymentGateway.Core.Services.BankService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "PaymentGateway/1.0");
});

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentValidator, PaymentValidator>();
builder.Services.AddScoped<IPaymentOrchestrator, PaymentGateway.Core.Services.PaymentOrchestrator>();
builder.Services.AddScoped<ITransactionRepository, PaymentGateway.Infrastructure.Repositories.TransactionRepository>();
builder.Services.AddScoped<ITransactionEventRepository, PaymentGateway.Infrastructure.Repositories.TransactionEventRepository>();
builder.Services.AddHostedService<PaymentGateway.Infrastructure.Services.TransactionRecoveryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }