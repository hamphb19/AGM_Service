using AGM_API.Database;
using AGM_API.Services.KnownObjects;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cors Settings
var AllowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AGM_API.Services.FarmAuthorizationService>();
builder.Services.AddScoped<AGM_API.Services.ActivityLogService>();

// -------- Keycloak (OIDC resource server) --------
// The API no longer issues tokens; it validates access tokens minted by Keycloak.
var keycloak = builder.Configuration.GetSection("Keycloak");
var keycloakAuthority = keycloak["Authority"];   // e.g. https://agm-auth.up.railway.app/realms/agm
var keycloakAudience = keycloak["Audience"] ?? "agm-api";
if (string.IsNullOrWhiteSpace(keycloakAuthority))
    throw new InvalidOperationException("Keycloak:Authority is not configured. Set it via environment variable Keycloak__Authority.");

builder.Services.AddScoped<AGM_API.Services.KeycloakUserProvisioningService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("isAdmin", "true"));
});

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = keycloakAuthority;
    options.Audience = keycloakAudience;
    options.RequireHttpsMetadata = true;
    // Keep the raw Keycloak claim names (sub, preferred_username, ...) instead of
    // remapping them to the legacy Microsoft claim URIs.
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = keycloakAuthority,
        ValidateAudience = true,
        ValidAudience = keycloakAudience,
        ValidateLifetime = true,
        NameClaimType = "preferred_username",
        RoleClaimType = "roles",
    };
});

// -------- HttpContextAccessor --------
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("AllowFrontend", policy =>
    {
        if (AllowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// -------- EF Core / Npgsql --------
builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Local"), o => o.UseNetTopologySuite());
});

var app = builder.Build();

// Auto-migrate + seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    var xml = Path.Combine(AppContext.BaseDirectory, "Services", "KnownObjects", "KnownObjects.xml");
    KnownObjectsService.LoadFromKnownObjectsXml(context, xml);

    KnownCacheService.Initialize(app.Services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseStaticFiles();

app.UseAuthentication();
// Map the Keycloak identity to a local User (auto-provision) and expose the
// local user id as NameIdentifier so existing controllers keep working.
app.UseMiddleware<AGM_API.Middleware.KeycloakUserMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
