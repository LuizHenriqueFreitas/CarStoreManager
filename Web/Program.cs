using CarStoreManager.Infrastructure.DependencyInjection;
using CarStoreManager.Web.Extensions;
using CarStoreManager.Web;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Infrastructure.Services;
using CarStoreManager.Application.Services;
using CarStoreManager.Infrastructure.Repositories;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Application.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.ValueObjects;
using Microsoft.AspNetCore.Components.Authorization;
using CarStoreManager.Web.Components.Shared;
using CarStoreManager.Web.Imports;
using CarStoreManager.Web.Components.Shared.Auth;

var builder = WebApplication.CreateBuilder(args);

// =========================
// BLAZOR
// =========================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// =========================
// SERVICES E INFRASTRUCTURE
// =========================
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// =========================
// JWT
// =========================
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

// "Smart" scheme: se o request tem header Authorization: Bearer, usa JwtBearer.
// Caso contrário, usa Cookie. Assim o mesmo controller serve API REST + Razor.
const string SmartScheme = "Smart";
builder.Services.AddAuthentication(SmartScheme)
    .AddPolicyScheme(SmartScheme, "Bearer ou Cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            return authHeader?.StartsWith("Bearer ") == true
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/login?logout=true";
        options.AccessDeniedPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromHours(jwtSettings.ExpiracaoHoras);
        options.SlidingExpiration = true;
        options.Cookie.Name = "carstore.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

// Authorize por padrão usa o "Smart" scheme — que decide Cookie vs JwtBearer.
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(SmartScheme)
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddHttpContextAccessor();

// =========================
// AUTH STATE PROVIDER (BLAZOR SERVER)
// =========================
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthStateProvider>());

// =========================
// CONTROLLERS (API)
// =========================
builder.Services.AddControllers();

// =========================
// BUILD
// =========================
var app = builder.Build();

// =========================
// SEED DO BANCO
// =========================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await SeedAdminAsync(context);
}

async Task SeedAdminAsync(AppDbContext context)
{
    var adminExiste = context.Usuarios.OfType<Admin>().Any();
    if (adminExiste) return;

    var admin = new Admin(
        "Administrador",
        "admin@teste.com",
        "11215126548",
        "12345A",
        6000
    );

    context.Usuarios.Add(admin);
    await context.SaveChangesAsync();

    Console.WriteLine("Admin criado — email: admin@teste.com / senha: 12345A");
}

// =========================
// PIPELINE
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Expõe a classe Program para que WebApplicationFactory<Program> consiga referenciá-la nos testes.
public partial class Program { }