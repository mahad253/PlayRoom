using GamingPlatform.Hubs;
using Microsoft.EntityFrameworkCore;
using GamingPlatform.Data;
using GamingPlatform.Services;
using GamingPlatform.Models;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Configuration du contexte de base de données
builder.Services.AddDbContext<GamingPlatformContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GamingPlatformContext") ?? 
        throw new InvalidOperationException("Connection string 'GamingPlatformContext' not found.")));

// Ajout des services au conteneur
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
	options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 Mo
});

builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<LobbyService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<GameSeeder>();
builder.Services.AddScoped<LabyrinthService>();
builder.Services.AddSingleton<SpeedTyping>();
builder.Services.AddSingleton(new ConcurrentDictionary<string, string>());

builder.Services.AddSession(options =>
{

    options.IdleTimeout = TimeSpan.FromMinutes(90); // Dur�e de la session
    options.Cookie.HttpOnly = true; // S�curiser le cookie

});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
});


var app = builder.Build();

// Configurez le pipeline des requêtes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<GameSeeder>();
    seeder.SeedGames();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();
app.UseAuthorization();

// Mapping des routes de contrôleur
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "game-details",
    pattern: "game/details/{id:guid}",
    defaults: new { controller = "Game", action = "Details" });

app.MapControllerRoute(
    name: "create-lobby-select",
    pattern: "lobby/create/select",
    defaults: new { controller = "Lobby", action = "CreateWithSelect" });

app.MapControllerRoute(
    name: "create-lobby-game",
    pattern: "lobby/create/game/{gameCode}",
    defaults: new { controller = "Lobby", action = "CreateFromGame" });

app.MapControllerRoute(
    name: "gameLobbies",
    pattern: "game/{gameCode}/lobbies",
    defaults: new { controller = "Game", action = "LobbiesByGameCode" });


app.MapHub<LabyrinthHub>("/labyrinthHub");
app.MapHub<MorpionHub>("/MorpionHub");

// Redirection pour le speed Typing game
app.MapHub<SpeedTypingHub>("/SpeedTypingHub");

// Nouvelle route pour PetitBacController.Configure
app.MapControllerRoute(
    name: "petitbac-configure",
    pattern: "petitbac/configure",
    defaults: new { controller = "PetitBac", action = "Configure" });

app.MapControllerRoute(
    name: "recapitulatif",
    pattern: "PetitBac/Recapitulatif/{gameId}",
    defaults: new { controller = "PetitBac", action = "Recapitulatif" });


// Ajout des hubs SignalR
app.MapHub<PetitBacHub>("/petitbachub"); 

// Lancer l'application
app.Run();