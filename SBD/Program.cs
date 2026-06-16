using SBD.Database;

var builder = WebApplication.CreateBuilder(args);

// Dodaj usługi MVC
builder.Services.AddControllersWithViews();

// Ustaw ConnectionString dla DbConnection z konfiguracji
var connectionString = builder.Configuration.GetConnectionString("SqlDb");
DbConnection.ConnectionString = connectionString;

var app = builder.Build();

// Konfiguracja potoku żądań HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
