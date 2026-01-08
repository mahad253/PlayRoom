using GamingPlatform.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// SERVICES
// =======================

// Database
builder.Services.AddDbContext<GamingPlatformContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("GamingPlatformContext")
        ?? throw new InvalidOperationException(
            "Connection string 'GamingPlatformContext' not found."
        )
    )
);

builder.Services.AddScoped<SpeedTypingService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// pour JS / CSS
app.UseStaticFiles();


// Seed JSON 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GamingPlatformContext>();
    // Si tu utilises Migrate :
    db.Database.Migrate();
    await SentenceSeeder.SeedAsync(db);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();