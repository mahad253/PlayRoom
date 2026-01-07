using GamingPlatform.Data;
using GamingPlatform.Hubs;
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

// =======================
// SIGNALR HUBS
// =======================

// Chat global (si tu le gardes)
app.MapHub<ChatHub>("/chatHub");

// Morpion (IMPORTANT)
app.MapHub<MorpionHub>("/morpionHub");

// =======================
// RUN
// =======================

app.Run();
