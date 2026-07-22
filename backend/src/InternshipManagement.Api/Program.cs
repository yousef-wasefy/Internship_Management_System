using System.Text.Json.Serialization;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    // Serialize enums as their names ("Open") instead of raw numbers (1) - much easier
    // to read and test against in Swagger/Postman.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Swashbuckle generates the OpenAPI document and serves the interactive Swagger UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In Development, the connection string comes from .NET User Secrets (never committed
// to git) - see docs/DECISIONS.md D11. appsettings.Development.json only holds a
// placeholder so the app fails fast with a clear error if secrets aren't configured.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IInternshipService, InternshipService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // TEMPORARY: seeds one placeholder company so internship posts have a valid
    // CompanyId before authentication exists - see Data/SeedData.cs, removed in Phase 8.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.EnsureSeededAsync(db);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
