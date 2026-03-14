using CarStoreManager.Web.Middlewares;
using CarStoreManager.Web.Extensions;
using CarStoreManager.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Registrar serviços
// ======================

// Controllers
builder.Services.AddControllers();

// Application Services
builder.Services.AddApplicationServices();

// Infrastructure (DbContext + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ======================
// Middlewares
// ======================

// Middleware global de exceções
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

// Mapear controllers
app.MapControllers();

app.Run();