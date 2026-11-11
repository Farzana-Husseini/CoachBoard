using CoachBoard.Data;
using CoachBoard.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    var p = options.ModelBindingMessageProvider;
    p.SetValueMustNotBeNullAccessor(_ => "Dieses Feld ist erforderlich.");
    p.SetAttemptedValueIsInvalidAccessor((wert, feld) =>
        $"'{wert}' ist kein gültiger Wert für {feld}. Datum: TT.MM.JJJJ, Uhrzeit: HH:MM.");
    p.SetValueIsInvalidAccessor(wert => $"'{wert}' ist ungültig.");
    p.SetUnknownValueIsInvalidAccessor(feld => $"Der Wert für {feld} ist ungültig.");
    p.SetMissingBindRequiredValueAccessor(feld => $"Für {feld} fehlt ein Wert.");
});

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// DB migrieren + ersten Trainer anlegen
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Role == AppRoles.Trainer))
    {
        var trainer = new User
        {
            Vorname = "Admin",
            Nachname = "Trainer",
            Email = "trainer@example.com",
            Role = AppRoles.Trainer
        };
        trainer.PasswordHash = hasher.HashPassword(trainer, "Trainer123!");
        db.Users.Add(trainer);
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // vor UseAuthorization!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();