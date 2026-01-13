using GamingPlatform.Hubs;
using GamingPlatform.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddSignalR();

builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();


builder.Services.AddSingleton<LobbyService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<Connect4Hub>("/connect4Hub");


app.MapHub<MorpionHub>("/morpionHub");

app.MapHub<SpeedTypingHub>("/speedTypingHub");

app.Run();
