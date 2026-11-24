using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Wishly.Infrastructure.Persistence;
using Wishly.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<WishlyDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("WishlyDb"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("WishlyDb"))
    )
);

builder.Services.AddHttpClient<LinkPreviewService>();
builder.Services.AddScoped<OrganisationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WishlistService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key manquante ou trop courte (32 caractères minimum).");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "wishly",
            ValidateAudience = true,
            ValidAudience = "wishly",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// Limite : 10 requêtes par minute et par adresse IP sur les routes de connexion
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://wishly.runasp.net",
                "https://wishly.runasp.net")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Détail des erreurs visible uniquement en local
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        try { await next(); }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(ex.ToString());
        }
    });

    // Redirection HTTPS seulement en local (Monster n'a que le HTTP)
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();