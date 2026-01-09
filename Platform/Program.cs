using GamingPlatform.Hubs;
using GamingPlatform.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// SERVICES
// =======================

// MVC
builder.Services.AddControllersWithViews();

// SignalR
builder.Services.AddSignalR();

// 🔥 LOBBY SERVICE (CERVEAU DU PROJET)
builder.Services.AddSingleton<LobbyService>();

var app = builder.Build();

// =======================
// MIDDLEWARE
// =======================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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

// Morpion (par lobby)
app.MapHub<MorpionHub>("/morpionHub");

// =======================
// RUN
// =======================

app.Run();