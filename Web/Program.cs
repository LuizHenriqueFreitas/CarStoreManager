using CarStoreManager.Infrastructure.DependencyInjection;
using CarStoreManager.Web.Extensions;
using CarStoreManager.Web;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Infrastructure.Services;
using CarStoreManager.Application.Services;
using CarStoreManager.Infrastructure.Repositories;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.ValueObjects;
using Microsoft.AspNetCore.Components.Authorization;
using CarStoreManager.Web.Components.Shared;
using CarStoreManager.Web.Imports;

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

builder.Services.AddAuthorization();

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
        new Email("admin@carstoremanager.com"),
        new Telefone("00000000000"),
        BCrypt.Net.BCrypt.HashPassword("Admin@123")
    );

    context.Usuarios.Add(admin);
    await context.SaveChangesAsync();

    Console.WriteLine("Admin criado — email: admin@carstoremanager.com / senha: Admin@123");
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