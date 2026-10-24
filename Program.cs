using AGM_API.Database;
using AGM_API.Services.SeedData;
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
builder.Services.AddSingleton<AGM_API.Services.FileStorageService>();
builder.Services.AddScoped<AGM_API.Services.AccountDeletionService>();
builder.Services.AddScoped<AGM_API.Services.KeycloakAdminService>();
builder.Services.AddHttpClient();

// Railway terminiert TLS vor der App → Schema/Host aus X-Forwarded-* übernehmen (für korrekte https-URLs).
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                       | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

// -------- Authentication --------
// Keycloak:Enabled = true  -> validate Keycloak access tokens (QA / Prod).
// Keycloak:Enabled = false -> local dev, no Keycloak needed (DevAuthHandler
//                             authenticates every request as a fixed dev user).
var keycloak = builder.Configuration.GetSection("Keycloak");
var keycloakEnabled = keycloak.GetValue("Enabled", true);
var keycloakAudience = keycloak["Audience"] ?? "agm-api";

builder.Services.AddScoped<AGM_API.Services.KeycloakUserProvisioningService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, AGM_API.Services.KeycloakClaimsTransformation>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("isAdmin", "true"));
});

if (keycloakEnabled)
{
    var keycloakAuthority = keycloak["Authority"];   // e.g. https://agm-auth.up.railway.app/realms/agm
    if (string.IsNullOrWhiteSpace(keycloakAuthority))
        throw new InvalidOperationException("Keycloak:Authority is not configured. Set it via environment variable Keycloak__Authority (or set Keycloak:Enabled=false for local dev).");

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
}
else
{
    Console.WriteLine("⚠ Keycloak DISABLED (Keycloak:Enabled=false) — using local DevAuth. Do NOT use in production.");
    builder.Services.AddAuthentication(AGM_API.Services.Auth.DevAuthHandler.SchemeName)
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, AGM_API.Services.Auth.DevAuthHandler>(
            AGM_API.Services.Auth.DevAuthHandler.SchemeName, null);
}

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

app.UseForwardedHeaders();

// Turn a missing/unresolvable auth claim (e.g. User.GetUserId() when KeycloakClaimsTransformation
// couldn't resolve a subject) into a clean 401 instead of an unhandled 500.
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (UnauthorizedAccessException)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
    }
});

// Auto-migrate + seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    var seedPath = Path.Combine(AppContext.BaseDirectory, "Services", "SeedData", "SeedData.json");
    SeedDataService.LoadFromJson(context, seedPath);

    SeedCacheService.Initialize(app.Services);

    // Upload-Verzeichnis anlegen und alte Dateien aus wwwroot/uploads einmalig übernehmen.
    _ = scope.ServiceProvider.GetRequiredService<AGM_API.Services.FileStorageService>();
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
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
