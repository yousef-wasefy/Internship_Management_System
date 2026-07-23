using System.Text;
using System.Text.Json.Serialization;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    // Serialize enums as their names ("Open") instead of raw numbers (1) - much easier
    // to read and test against in Swagger/Postman.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Swashbuckle generates the OpenAPI document and serves the interactive Swagger UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adds the "Authorize" button to Swagger so a JWT can be attached to requests.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token returned by /api/auth/login or /api/auth/register-* here (no 'Bearer ' prefix needed)."
    });
    // Microsoft.OpenApi 2.x reworked references: a security requirement is now built
    // from a callback that receives the in-progress OpenApiDocument, used to construct
    // a reference to the "Bearer" scheme defined just above.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// In Development, the connection string comes from .NET User Secrets (never committed
// to git) - see docs/DECISIONS.md D11. appsettings.Development.json only holds a
// placeholder so the app fails fast with a clear error if secrets aren't configured.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IInternshipService, InternshipService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

// The JWT signing key lives in User Secrets, same as the DB password - see D11/D12.
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured - set it via dotnet user-secrets.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // MapInboundClaims = false keeps claim names exactly as JwtTokenGenerator wrote
        // them ("sub", not a remapped long URI) - predictable, no silent ASP.NET Core
        // claim-type remapping to debug around.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seeds a placeholder company (temporary, see Data/SeedData.cs) and an admin
    // account so there's something to log in with while testing.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.EnsureSeededAsync(db);
}

app.UseHttpsRedirection();

// Authentication (who are you?) must run before authorization (are you allowed to?).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
