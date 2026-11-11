
using System.Security.Claims;
using CoachBoard.Data;
using CoachBoard.Models;
using CoachBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Controllers
{
    [Authorize]
    public class AnwesenheitController : Controller
    {
        private readonly AppDbContext _db;

        // false = Alle Trainingseinheiten werden für die Quote gezählt.
        // true = Nur bereits begonnene Trainingseinheiten werden gezählt.
        private static readonly bool NurStattgefundeneZaehlen = false;

        public AnwesenheitController(AppDbContext db)
        {
            _db = db;
        }

        // ID des aktuell angemeldeten Benutzers
        private int CurrentUserId
        {
            get
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.Parse(userId);
            }
        }

        // Trainer werden direkt zur Anwesenheitsquote weitergeleitet.
        [Authorize(Roles = AppRoles.Trainer)]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Quote));
        }

        // Spieler können ihren Anwesenheitsstatus setzen oder ändern.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Spieler)]
        public async Task<IActionResult> Setzen(int trainingId, AnwesenheitsStatus status)
        {
            if (!Enum.IsDefined(status))
            {
                return BadRequest();
            }

            bool trainingExistiert = await _db.Trainingseinheiten
                .AnyAsync(t => t.Id == trainingId);

            if (!trainingExistiert)
            {
                return NotFound();
            }

            int userId = CurrentUserId;

            // Zwei Versuche, falls zwei Anfragen gleichzeitig einen Eintrag erstellen.
            // Der Unique-Index verhindert dabei doppelte Einträge.
            for (int versuch = 0; versuch < 2; versuch++)
            {
                var eintrag = await _db.Anwesenheiten
                    .FirstOrDefaultAsync(a =>
                        a.TrainingseinheitId == trainingId &&
                        a.UserId == userId);

                if (eintrag == null)
                {
                    Anwesenheit neueAnwesenheit = new Anwesenheit
                    {
                        TrainingseinheitId = trainingId,
                        UserId = userId,
                        Status = status,
                        GeaendertAm = DateTime.Now
                    };

                    _db.Anwesenheiten.Add(neueAnwesenheit);
                }
                else
                {
                    eintrag.Status = status;
                    eintrag.GeaendertAm = DateTime.Now;
                }

                try
                {
                    await _db.SaveChangesAsync();
                    break;
                }
                catch (DbUpdateException) when (versuch == 0)
                {
                    // Änderungen zurücksetzen und den Vorgang erneut versuchen.
                    _db.ChangeTracker.Clear();
                }
            }

            TempData["Success"] = $"Dein Status wurde auf \"{status}\" gesetzt.";

            return RedirectToAction("Details", "Training", new { id = trainingId });
        }

        // Spieler sehen ihre eigene Trainingshistorie.
        [Authorize(Roles = AppRoles.Spieler)]
        public async Task<IActionResult> Meine(int page = 1)
        {
            const int seitenGroesse = 10;

            int userId = CurrentUserId;

            var trainings = _db.Trainingseinheiten.AsQueryable();

            if (NurStattgefundeneZaehlen)
            {
                trainings = trainings.Where(t => t.Beginn <= DateTime.Now);
            }

            List<int> trainingIds = await trainings
                .Select(t => t.Id)
                .ToListAsync();

            int gesamt = trainingIds.Count;

            var meineAnwesenheiten = await _db.Anwesenheiten
                .Where(a => a.UserId == userId &&
                            trainingIds.Contains(a.TrainingseinheitId))
                .ToListAsync();

            // Anzahl der benötigten Seiten berechnen.
            int seiten = Math.Max(
                1,
                (int)Math.Ceiling(gesamt / (double)seitenGroesse)
            );

            // Die aktuelle Seite darf nicht außerhalb des gültigen Bereichs liegen.
            if (page < 1)
            {
                page = 1;
            }

            if (page > seiten)
            {
                page = seiten;
            }

            // Die neuesten Trainingseinheiten zuerst anzeigen.
            var trainingsAufSeite = await trainings
                .OrderByDescending(t => t.Beginn)
                .Skip((page - 1) * seitenGroesse)
                .Take(seitenGroesse)
                .ToListAsync();

            var statusProTraining = meineAnwesenheiten
                .ToDictionary(a => a.TrainingseinheitId, a => a.Status);

            MeineHistorieViewModel vm = new MeineHistorieViewModel
            {
                Zeilen = trainingsAufSeite.Select(t =>
                {
                    statusProTraining.TryGetValue(t.Id, out var status);

                    return new HistorieZeile
                    {
                        Training = t,
                        Status = statusProTraining.ContainsKey(t.Id) ? status : null
                    };
                }).ToList(),

                Page = page,
                TotalPages = seiten,
                Gesamt = gesamt,

                Anwesend = meineAnwesenheiten.Count(
                    a => a.Status == AnwesenheitsStatus.Anwesend),

                Abwesend = meineAnwesenheiten.Count(
                    a => a.Status == AnwesenheitsStatus.Abwesend),

                Entschuldigt = meineAnwesenheiten.Count(
                    a => a.Status == AnwesenheitsStatus.Entschuldigt),

                NurStattgefundene = NurStattgefundeneZaehlen
            };

            return View(vm);
        }

        // Trainer sehen die Anwesenheit aller Spieler einer Trainingseinheit.
        [Authorize(Roles = AppRoles.Trainer)]
        public async Task<IActionResult> Einheit(int id)
        {
            var training = await _db.Trainingseinheiten.FindAsync(id);

            if (training == null)
            {
                return NotFound();
            }

            var spieler = await _db.Users
                .Where(u => u.Role == AppRoles.Spieler)
                .OrderBy(u => u.Nachname)
                .ThenBy(u => u.Vorname)
                .ToListAsync();

            var anwesenheiten = await _db.Anwesenheiten
                .Where(a => a.TrainingseinheitId == id)
                .ToDictionaryAsync(a => a.UserId);

            EinheitAnwesenheitViewModel vm = new EinheitAnwesenheitViewModel
            {
                Training = training,

                Zeilen = spieler.Select(s =>
                {
                    anwesenheiten.TryGetValue(s.Id, out var eintrag);

                    return new AnwesenheitZeile
                    {
                        Name = s.VollerName,
                        Email = s.Email,
                        Status = eintrag?.Status,
                        GeaendertAm = eintrag?.GeaendertAm
                    };
                }).ToList()
            };

            return View(vm);
        }

        // Trainer sehen die Anwesenheitsquote aller Spieler.
        [Authorize(Roles = AppRoles.Trainer)]
        public async Task<IActionResult> Quote()
        {
            var trainings = _db.Trainingseinheiten.AsQueryable();

            if (NurStattgefundeneZaehlen)
            {
                trainings = trainings.Where(t => t.Beginn <= DateTime.Now);
            }

            List<int> trainingIds = await trainings
                .Select(t => t.Id)
                .ToListAsync();

            int gesamt = trainingIds.Count;

            var spieler = await _db.Users
                .Where(u => u.Role == AppRoles.Spieler)
                .OrderBy(u => u.Nachname)
                .ThenBy(u => u.Vorname)
                .ToListAsync();

            var anwesenheiten = await _db.Anwesenheiten
                .Where(a => trainingIds.Contains(a.TrainingseinheitId))
                .ToListAsync();

            // Anwesenheitseinträge nach Spieler gruppieren.
            var anwesenheitenProSpieler = anwesenheiten
                .GroupBy(a => a.UserId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var zeilen = spieler.Select(s =>
            {
                List<Anwesenheit> eintraege;

                if (!anwesenheitenProSpieler.TryGetValue(s.Id, out eintraege))
                {
                    eintraege = new List<Anwesenheit>();
                }

                int anwesend = eintraege.Count(
                    a => a.Status == AnwesenheitsStatus.Anwesend);

                int abwesend = eintraege.Count(
                    a => a.Status == AnwesenheitsStatus.Abwesend);

                int entschuldigt = eintraege.Count(
                    a => a.Status == AnwesenheitsStatus.Entschuldigt);

                return new QuoteZeile
                {
                    Name = s.VollerName,
                    Email = s.Email,
                    Anwesend = anwesend,
                    Abwesend = abwesend,
                    Entschuldigt = entschuldigt,
                    Offen = gesamt - anwesend - abwesend - entschuldigt,
                    Gesamt = gesamt
                };
            })
            .OrderByDescending(z => z.Quote)
            .ThenBy(z => z.Name)
            .ToList();

            QuoteViewModel vm = new QuoteViewModel
            {
                Zeilen = zeilen,
                AnzahlTrainings = gesamt,
                NurStattgefundene = NurStattgefundeneZaehlen
            };

            return View(vm);
        }
    }
}
