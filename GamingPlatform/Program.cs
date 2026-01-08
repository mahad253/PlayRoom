using GamingPlatform.Data;
using GamingPlatform.Hubs;
using GamingPlatform.Services;
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
            "Connection string 'ApplicationDbContext' not found."
        )
    )
);

// MVC
builder.Services.AddControllersWithViews();

// SignalR
builder.Services.AddSignalR();

// Store des jeux (Puissance 4, Morpion, etc.)
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();

builder.Services.AddScoped<SpeedTypingService>();

var app = builder.Build();

// =======================
// MIDDLEWARE
// =======================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS (peut être commenté en local si besoin)
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// =======================
// ROUTES MVC
// =======================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// =======================
// SIGNALR HUBS
// =======================

// Chat global
app.MapHub<ChatHub>("/chatHub");

// Puissance 4
app.MapHub<Connect4Hub>("/connect4Hub");

// Morpion
app.MapHub<MorpionHub>("/morpionHub");

// =======================
// DATABASE AUTO-MIGRATION
// =======================
// Applique automatiquement les migrations + HasData()

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GamingPlatformContext>();
        db.Database.Migrate();
    }
}

// =======================
// RUN
// =======================

app.Run();