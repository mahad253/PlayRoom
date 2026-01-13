using GamingPlatform.Hubs;
using GamingPlatform.Services;

var builder = WebApplication.CreateBuilder(args);

// =======================
// SERVICES
// =======================

// MVC
builder.Services.AddControllersWithViews();

// ✅ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SignalR
builder.Services.AddSignalR();

// ✅ Store des jeux (Puissance 4, etc.)
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();

// ✅ Lobby service (utilisé par Morpion sur main)
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

// ✅ Session
app.UseSession();

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

app.Run();
