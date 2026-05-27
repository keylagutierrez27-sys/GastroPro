using Microsoft.EntityFrameworkCore;
using GastroPro.Infrastructure.Data;
using GastroPro.Domain.Interfaces;
using GastroPro.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar Controladores y Vistas de MVC
builder.Services.AddControllersWithViews();

// 2. Configurar la conexión a SQL Server vinculando nuestro DbContext de Infraestructura
builder.Services.AddDbContext<GastroProDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Registrar el puente UnitOfWork (Arquitectura Limpia)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 4. Registrar los servicios de sesión en memoria
builder.Services.AddSession();

// Necesario para que el layout pueda acceder a HttpContextAccessor desde las vistas Razor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Asegura el mapeo correcto de estilos y scripts

app.UseRouting();

// 🛑 PASO CRÍTICO: Activamos el middleware de sesiones en el orden correcto
app.UseSession();

app.UseAuthorization();
app.MapStaticAssets();

// Ruta por defecto: Arranca en el Home Controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();