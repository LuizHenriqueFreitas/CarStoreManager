using CarStoreManager.Infrastructure.DependencyInjection;
using CarStoreManager.Web.Extensions;
using CarStoreManager.Web;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Serviços do Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 🔹 Injeção das camadas
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// 🔹 Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

// 🔹 Mapeamento do Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();