
using System.Security.Claims;
using CoachBoard.Data;
using CoachBoard.Models;
using CoachBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Controllers
{
    [Authorize(Roles = AppRoles.Trainer)]
    public class SpielerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        // Diese Rollen dürfen bei Benutzern ausgewählt werden.
        private static readonly string[] ErlaubteRollen =
        {
            AppRoles.Spieler,
            AppRoles.Trainer
        };

        public SpielerController(AppDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        // ID des aktuell angemeldeten Benutzers.
        private int CurrentUserId
        {
            get
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.Parse(userId);
            }
        }

        // Alle Benutzer anzeigen.
        public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Nachname)
                .ThenBy(u => u.Vorname)
                .ToListAsync();

            return View(users);
        }

        // Formular zum Erstellen eines neuen Benutzers anzeigen.
        public IActionResult Create()
        {
            SpielerCreateViewModel model = new SpielerCreateViewModel();

            return View(model);
        }

        // Neuen Benutzer speichern.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpielerCreateViewModel model)
        {
            string email = model.Email.Trim().ToLowerInvariant();

            // Prüfen, ob die E-Mail bereits verwendet wird.
            bool emailExistiert = await _db.Users
                .AnyAsync(u => u.Email == email);

            if (emailExistiert)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Diese E-Mail ist bereits vergeben."
                );
            }

            // Prüfen, ob eine erlaubte Rolle ausgewählt wurde.
            if (!ErlaubteRollen.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Ungültige Rolle."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = new User
            {
                Vorname = model.Vorname.Trim(),
                Nachname = model.Nachname.Trim(),
                Email = email,
                Role = model.Role
            };

            // Passwort verschlüsseln und als Hash speichern.
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _db.Users.Add(user);

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{model.Role} wurde angelegt.";

            return RedirectToAction(nameof(Index));
        }

        // Formular zum Bearbeiten eines Benutzers anzeigen.
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            SpielerEditViewModel model = new SpielerEditViewModel
            {
                Id = user.Id,
                Vorname = user.Vorname,
                Nachname = user.Nachname,
                Email = user.Email,
                Role = user.Role
            };

            return View(model);
        }

        // Änderungen am Benutzer speichern.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SpielerEditViewModel model)
        {
            var user = await _db.Users.FindAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            string email = model.Email.Trim().ToLowerInvariant();

            // Prüfen, ob die E-Mail bereits von einem anderen Benutzer verwendet wird.
            bool emailExistiert = await _db.Users
                .AnyAsync(u => u.Email == email && u.Id != model.Id);

            if (emailExistiert)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Diese E-Mail ist bereits vergeben."
                );
            }

            // Prüfen, ob die ausgewählte Rolle erlaubt ist.
            if (!ErlaubteRollen.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Ungültige Rolle."
                );
            }

            // Ein Benutzer darf seine eigene Rolle nicht ändern.
            if (user.Id == CurrentUserId && model.Role != user.Role)
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Du kannst deine eigene Rolle nicht ändern."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.Vorname = model.Vorname.Trim();
            user.Nachname = model.Nachname.Trim();
            user.Email = email;
            user.Role = model.Role;

            // Nur ein neues Passwort speichern, wenn eines eingegeben wurde.
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = _passwordHasher.HashPassword(
                    user,
                    model.NewPassword
                );
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Benutzer wurde aktualisiert.";

            return RedirectToAction(nameof(Index));
        }

        // Bestätigungsseite zum Löschen eines Benutzers anzeigen.
        public async Task<IActionResult> Delete(int id)
        {
            if (id == CurrentUserId)
            {
                TempData["Error"] = "Du kannst dich nicht selbst löschen.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Benutzer endgültig löschen.
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == CurrentUserId)
            {
                TempData["Error"] = "Du kannst dich nicht selbst löschen.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _db.Users.FindAsync(id);

            if (user != null)
            {
                _db.Users.Remove(user);

                await _db.SaveChangesAsync();

                TempData["Success"] = "Benutzer wurde gelöscht.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
