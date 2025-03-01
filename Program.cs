using PaymentsMS.Application.Interfaces;
using PaymentsMS.Application.Mapping;
using PaymentsMS.Application.Services;
using PaymentsMS.Infrastructure.Auth;
using PaymentsMS.Infrastructure.Data;
using PaymentsMS.Infrastructure.EventBus;
using PaymentsMS.Infrastructure.Repository;
using PaymentsMS.Infrastructure.Swagger;
using Stripe;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddNpgsql<TransactionsDbContext>(builder.Configuration.GetConnectionString("dbConnectionTransactions"));

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<EnumSchemaFilter>(); // Enables los enums as string
});

builder.Services.AddScoped(typeof(ICreateRepository<>), typeof(CreateRepository<>));
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<ITransactionsService, TransactionsService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISessionStripe, SessionStripe>();
builder.Services.AddScoped(provider => new Lazy<ITransactionsService>(() => provider.GetRequiredService<ITransactionsService>())); 
builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

builder.AddAppAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

StripeConfiguration.ApiKey = builder.Configuration.GetValue<string>("Stripe:Secret");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
