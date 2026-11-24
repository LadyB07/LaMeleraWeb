var builder = WebApplication.CreateBuilder(args);

// Agregar servicios MVC, Capa de Datos y Sesiones
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<LaMeleraWeb.Models.MeleraDAL>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession(); // <-- Activa la sesión del dueño
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();