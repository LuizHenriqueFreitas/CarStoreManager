using CarStoreManager.Infrastructure.DependencyInjection;
using CarStoreManager.Web.Extensions;
using CarStoreManager.Application.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CarStoreManager.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using CarStoreManager.Web.Imports;
using CarStoreManager.Web.Components.Shared.Auth;

var builder = WebApplication.CreateBuilder(args);

// =========================
// BLAZOR
// =========================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Circuito Blazor mais tolerante a instabilidade de rede/idle — evita que o
// banner de "conexão perdida" (#blazor-error-ui) apareça por uma oscilação
// passageira de wifi ou por a aba ficar parada um tempo durante uma
// demonstração. Valores acima do padrão do framework (30s/15s de timeout de
// hub, 3min/100 de retenção de circuito desconectado).
builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(20);
});
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.DisconnectedCircuitMaxRetained = 200;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
});

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

// =========================
// DATA PROTECTION (criptografia dos tokens OAuth do Mercado Livre)
// =========================
// Nenhuma persistência explícita de chaves existia antes desta integração — o
// anel de chaves padrão implícito não é garantido estável entre reinícios/redeploys.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
    .SetApplicationName("CarStoreManager");

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
// Policies dinâmicas por recurso do catálogo (doc 37 — acesso individual/alçada):
// troca o RolesAuthorizationRequirement estático por uma checagem que
// também consulta exceções pessoais (PermissaoIndividual), sem mudar o UX.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, CarStoreManager.Web.Authorization.RecursoPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, CarStoreManager.Web.Authorization.RecursoAcessoHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CarStoreManager.Web.Authorization.ModuloAcessoHandler>();
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
// Pulado no ambiente "Testing" — é o que WebApplicationFactory usa nos testes
// de integração de controller (ver Tests/Integratrion/Controllers/*), que
// mockam a camada de serviço e nunca precisam de um Postgres de verdade no ar.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Backup automático ANTES das migrations/seed (protege contra migration
    // que estrague dado). Fica em <pasta do .db>/backups, mantém os 7 últimos.
    // Nunca pode impedir o app de subir.
    try
    {
        var backup = scope.ServiceProvider.GetRequiredService<CarStoreManager.Application.Interfaces.Sistema.IBackupBancoService>();
        var rb = await backup.GerarBackupAutomaticoAsync(manter: 7);
        if (rb.IsSuccess && rb.Value is not null)
            app.Logger.LogInformation("Backup automático gravado em {Caminho}", rb.Value);
        else if (!rb.IsSuccess)
            app.Logger.LogWarning("Backup automático não gerado: {Erro}", rb.Error);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Backup automático falhou — o sistema sobe mesmo assim.");
    }

    // Migrations + admin inicial + presets + templates padrão — extraído pra
    // Infrastructure/Data/SeedInicial.cs (o CLI de importação do Geradores
    // reaproveita exatamente o mesmo seed).
    await SeedInicial.AplicarAsync(context);
}

// =========================
// PIPELINE
// =========================
// Página de erro amigável sempre ativa — inclusive em Development. A stack
// trace crua do meio termo padrão do ASP.NET Core não é algo que um usuário
// leigo deva ver; o erro completo continua indo pro log (Console/ILogger)
// via UseExceptionHandler, só não é mais devolvido na resposta HTTP.
app.UseExceptionHandler("/Error");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

//app.UseHttpsRedirection();
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