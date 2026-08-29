using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Middleware;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    // Serialize enums as their names ("Open") instead of raw numbers (1) - much easier
    // to read and test against in Swagger/Postman.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Powers the automatic 400 responses [ApiController] already returns for failed
// DataAnnotations validation, AND gives Problem(...) calls in controllers (used for
// every business-rule error - see docs/DECISIONS.md D17) a consistent JSON shape.
builder.Services.AddProblemDetails();

// Catches anything that isn't an expected, handled outcome - see Middleware/GlobalExceptionHandler.cs.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// The React dev server (Phase 13) runs on a different origin (port) than the API, so the
// browser blocks its fetch calls without this. No AllowCredentials(): auth is a Bearer
// token in a header, not a cookie, so the stricter credentialed-CORS rules don't apply.
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Swashbuckle generates the OpenAPI document and serves the interactive Swagger UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Internship Management System API",
        Version = "v1",
        Description = "REST API for students, companies, and admins to manage internship " +
            "postings and applications. See docs/API_SPEC.md and docs/REQUIREMENTS.md in " +
            "the repository for the full business rules behind each endpoint."
    });

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

    // Picks up the <summary> XML comments written on the more business-rule-heavy
    // controller actions (Open/Close/Apply/Withdraw/UpdateStatus/Reject/Disable) -
    // requires <GenerateDocumentationFile> in the .csproj.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// In Development, the connection string comes from .NET User Secrets (never committed
// to git) - see docs/DECISIONS.md D11. In Docker (Phase 16), it comes from the
// ConnectionStrings__DefaultConnection environment variable instead - ASP.NET Core's
// configuration system maps that double-underscore name to this same config key
// automatically, so no code here needs to know which source it came from.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IInternshipService, InternshipService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IAdminService, AdminService>();

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

// Makes [Authorize] rejections (wrong/missing role, no token) return the same
// ProblemDetails body shape as every other error in the API - see
// Middleware/ProblemDetailsAuthorizationMiddlewareResultHandler.cs.
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationMiddlewareResultHandler>();

var app = builder.Build();

// Registered first so it wraps everything else in the pipeline - any unhandled
// exception from any middleware or controller below gets caught here.
app.UseExceptionHandler();

// Applies any pending EF Core migrations automatically, in every environment - this is
// what lets a fresh `docker compose up` (Phase 16) end up with a fully-formed schema
// with no manual `dotnet ef database update` step. Local development used to require
// running that command by hand after pulling a new migration; doing it here instead
// means there's exactly one way this ever happens, not two slightly-different ones to
// keep in sync. Seeding the admin account (idempotent - see SeedData.cs) runs right
// after, so a brand new environment has something to log in with immediately.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}

// Swagger stays available in every environment, including the Phase 16 Docker
// "production simulation" and the eventual Phase 17 deploy - unlike a real product,
// this is a portfolio project where letting anyone explore the API is the point, not a
// security surface to lock down. See docs/DECISIONS.md D21.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

// Authentication (who are you?) must run before authorization (are you allowed to?).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
