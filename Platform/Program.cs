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
            "Connection string 'GamingPlatformContext' not found."
        )
    )
);

// MVC
builder.Services.AddControllersWithViews();

// SignalR
builder.Services.AddSignalR();

// Store des jeux (Puissance 4, etc.)
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();

var app = builder.Build();

// =======================
// MIDDLEWARE
// =======================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Si vous ne gérez pas HTTPS en local, vous pouvez commenter cette ligne
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
// RUN
// =======================

app.Run();
