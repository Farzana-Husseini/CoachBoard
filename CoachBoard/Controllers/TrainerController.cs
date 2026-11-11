
using System.Globalization;
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
    public class TrainingController : Controller
    {
        private readonly AppDbContext _db;

        private const int SeitenGroesse = 10;

        public TrainingController(AppDbContext db)
        {
            _db = db;
        }

        // Alle Trainingseinheiten anzeigen.
        public async Task<IActionResult> Index(
            string? monat,
            string? zeitraum,
            string? status,
            int? spielerId,
            int page = 1)
        {
            DateTime jetzt = DateTime.Now;

            bool istTrainer = User.IsInRole(AppRoles.Trainer);
            bool istSpieler = User.IsInRole(AppRoles.Spieler);

            // Prüfen, welcher Zeitraum ausgewählt wurde.
            if (zeitraum != "kommende" && zeitraum != "vergangene")
            {
                zeitraum = "alle";
            }

            // Bestimmen, für welchen Spieler der Anwesenheitsstatus angezeigt wird.
            int? userId = null;

            if (istSpieler)
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                userId = int.Parse(id);
            }
            else if (istTrainer && spielerId.HasValue)
            {
                bool spielerExistiert = await _db.Users
                    .AnyAsync(u => u.Id == spielerId.Value &&
                                   u.Role == AppRoles.Spieler);

                if (spielerExistiert)
                {
                    userId = spielerId.Value;
                }
                else
                {
                    spielerId = null;
                }
            }
            else
            {
                spielerId = null;
            }

            var trainings = _db.Trainingseinheiten.AsQueryable();

            // Filter nach Monat.
            if (!string.IsNullOrWhiteSpace(monat))
            {
                DateTime monatStart;

                bool gueltigerMonat = DateTime.TryParseExact(
                    monat + "-01",
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out monatStart
                );

                if (gueltigerMonat)
                {
                    DateTime monatEnde = monatStart.AddMonths(1);

                    trainings = trainings.Where(t =>
                        t.Beginn >= monatStart && t.Beginn < monatEnde);
                }
                else
                {
                    monat = null;
                }
            }
            else
            {
                monat = null;
            }

            // Filter nach kommenden und vergangenen Trainingseinheiten.
            if (zeitraum == "kommende")
            {
                trainings = trainings.Where(t => t.Beginn >= jetzt);
            }
            else if (zeitraum == "vergangene")
            {
                trainings = trainings.Where(t => t.Beginn < jetzt);
            }

            // Filter nach Anwesenheitsstatus.
            // Dieser Filter wird nur verwendet, wenn ein Spieler ausgewählt ist.
            if (userId.HasValue && !string.IsNullOrEmpty(status))
            {
                int ausgewaehlteUserId = userId.Value;

                if (status == "Offen")
                {
                    trainings = trainings.Where(t =>
                        !_db.Anwesenheiten.Any(a =>
                            a.TrainingseinheitId == t.Id &&
                            a.UserId == ausgewaehlteUserId));
                }
                else
                {
                    AnwesenheitsStatus ausgewaehlterStatus;

                    bool gueltigerStatus = Enum.TryParse(
                        status,
                        out ausgewaehlterStatus
                    );

                    if (gueltigerStatus && Enum.IsDefined(ausgewaehlterStatus))
                    {
                        trainings = trainings.Where(t =>
                            _db.Anwesenheiten.Any(a =>
                                a.TrainingseinheitId == t.Id &&
                                a.UserId == ausgewaehlteUserId &&
                                a.Status == ausgewaehlterStatus));
                    }
                    else
                    {
                        status = null;
                    }
                }
            }
            else
            {
                status = null;
            }

            // Anzahl der Trainingseinheiten berechnen.
            int gesamt = await trainings.CountAsync();

            int seiten = Math.Max(
                1,
                (int)Math.Ceiling(gesamt / (double)SeitenGroesse)
            );

            // Prüfen, ob die ausgewählte Seite gültig ist.
            if (page < 1)
            {
                page = 1;
            }

            if (page > seiten)
            {
                page = seiten;
            }

            // Kommende Trainingseinheiten zuerst anzeigen.
            // Danach werden vergangene Einheiten angezeigt.
            var items = await trainings
                .OrderBy(t => t.Beginn < jetzt ? 1 : 0)
                .ThenBy(t => t.Beginn >= jetzt ? t.Beginn : DateTime.MaxValue)
                .ThenByDescending(t => t.Beginn)
                .Skip((page - 1) * SeitenGroesse)
                .Take(SeitenGroesse)
                .ToListAsync();

            // Daten für die View vorbereiten.
            TrainingIndexViewModel vm = new TrainingIndexViewModel
            {
                Items = items,
                Page = page,
                PageSize = SeitenGroesse,
                TotalCount = gesamt,
                Monat = monat,
                Zeitraum = zeitraum,
                Status = status,
                SpielerId = spielerId
            };

            // Trainer können einen Spieler für die Filterung auswählen.
            if (istTrainer)
            {
                vm.Spieler = await _db.Users
                    .Where(u => u.Role == AppRoles.Spieler)
                    .OrderBy(u => u.Nachname)
                    .ThenBy(u => u.Vorname)
                    .Select(u => new SpielerOption
                    {
                        Id = u.Id,
                        Name = u.Vorname + " " + u.Nachname
                    })
                    .ToListAsync();
            }

            // Anwesenheitsstatus für die aktuelle Seite laden.
            if (userId.HasValue)
            {
                List<int> trainingIds = items.Select(t => t.Id).ToList();

                vm.StatusProTraining = await _db.Anwesenheiten
                    .Where(a => a.UserId == userId.Value &&
                                trainingIds.Contains(a.TrainingseinheitId))
                    .ToDictionaryAsync(
                        a => a.TrainingseinheitId,
                        a => a.Status
                    );

                if (istSpieler)
                {
                    vm.StatusSpalte = "Meine Anwesenheit";
                }
                else
                {
                    var ausgewaehlterSpieler = vm.Spieler
                        .FirstOrDefault(s => s.Id == userId.Value);

                    vm.StatusSpalte = "Status: " + ausgewaehlterSpieler?.Name;
                }
            }

            return View(vm);
        }

        // Details einer Trainingseinheit anzeigen.
        public async Task<IActionResult> Details(int id)
        {
            var training = await _db.Trainingseinheiten.FindAsync(id);

            if (training == null)
            {
                return NotFound();
            }

            // Spieler sehen zusätzlich ihren eigenen Anwesenheitsstatus.
            if (User.IsInRole(AppRoles.Spieler))
            {
                string idText = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = int.Parse(idText);

                var anwesenheit = await _db.Anwesenheiten
                    .FirstOrDefaultAsync(a =>
                        a.TrainingseinheitId == id &&
                        a.UserId == userId);

                // null bedeutet, dass noch kein Status gesetzt wurde.
                ViewData["MeinStatus"] = anwesenheit?.Status;
            }

            return View(training);
        }

        // Neue Trainingseinheit anlegen.
        [Authorize(Roles = AppRoles.Trainer)]
        public IActionResult Create()
        {
            TrainingFormViewModel model = new TrainingFormViewModel
            {
                Datum = DateTime.Today
            };

            return View(model);
        }

        // Daten der neuen Trainingseinheit speichern.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Trainer)]
        public async Task<IActionResult> Create(TrainingFormViewModel model)
        {
            PruefeWerte(model, true);

            await PruefeDoppelbuchungAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Trainingseinheit training = new Trainingseinheit
            {
                Beginn = model.Datum!.Value.Date +
                         model.Uhrzeit!.Value.ToTimeSpan(),

                Ort = model.Ort.Trim(),
                Beschreibung = model.Beschreibung?.Trim(),
                GeplanteUebungen = model.GeplanteUebungen?.Trim()
            };

            _db.Trainingseinheiten.Add(training);

            await _db.SaveChangesAsync();

            TempData["Success"] = "Trainingseinheit wurde angelegt.";

            return RedirectToAction(nameof(Index));
        }

        // Formular zum Bearbeiten einer Trainingseinheit anzeigen.
        [Authorize(Roles = AppRoles.Trainer)]
        public async Task<IActionResult> Edit(int id)
        {
            var training = await _db.Trainingseinheiten.FindAsync(id);

            if (training == null)
            {
                return NotFound();
            }

            TrainingFormViewModel model = new TrainingFormViewModel
            {
                Id = training.Id,
                Datum = training.Beginn.Date,
                Uhrzeit = TimeOnly.FromDateTime(training.Beginn),
                Ort = training.Ort,
                Beschreibung = training.Beschreibung,
                GeplanteUebungen = training.GeplanteUebungen
            };

            return View(model);
        }

        // Änderungen an einer Trainingseinheit speichern.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Trainer)]
        public async Task<IActionResult> Edit(TrainingFormViewModel model)
        {
            var training = await _db.Trainingseinheiten.FindAsync(model.Id);

            if (training == null)
            {
                return NotFound();
            }

            PruefeWerte(model, false);

            await PruefeDoppelbuchungAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            training.Beginn = model.Datum!.Value.Date +
                              model.Uhrzeit!.Value.ToTimeSpan();

            training.Ort = model.Ort.Trim();
            training.Beschreibung = model.Beschreibung?.Trim();
            training.GeplanteUebungen = model.GeplanteUebungen?.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = "Trainingseinheit wurde aktualisiert.";

            return RedirectToAction(nameof(Index));
        }

        // Bestätigungsseite zum Löschen anzeigen.
        [Authorize(Roles = AppRoles.Trainer)]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _db.Trainingseinheiten.FindAsync(id);

            if (training == null)
            {
                return NotFound();
            }

            return View(training);
        }

        // Trainingseinheit löschen.
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Trainer)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var training = await _db.Trainingseinheiten.FindAsync(id);

            if (training != null)
            {
                _db.Trainingseinheiten.Remove(training);

                await _db.SaveChangesAsync();

                TempData["Success"] = "Trainingseinheit wurde gelöscht.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Datum und andere Werte überprüfen.
        private void PruefeWerte(TrainingFormViewModel model, bool istNeu)
        {
            if (model.Datum.HasValue)
            {
                int jahr = model.Datum.Value.Year;

                if (jahr < 2000 || jahr > 2100)
                {
                    ModelState.AddModelError(
                        nameof(model.Datum),
                        "Bitte ein Datum zwischen 2000 und 2100 angeben."
                    );
                }
                else if (istNeu && model.Datum.Value.Date < DateTime.Today)
                {
                    ModelState.AddModelError(
                        nameof(model.Datum),
                        "Das Datum darf nicht in der Vergangenheit liegen."
                    );
                }
            }
        }

        // Prüfen, ob am gleichen Ort und zur gleichen Zeit
        // bereits eine andere Trainingseinheit eingetragen ist.
        private async Task PruefeDoppelbuchungAsync(TrainingFormViewModel model)
        {
            // Nur prüfen, wenn alle notwendigen Angaben vorhanden sind.
            if (!model.Datum.HasValue ||
                !model.Uhrzeit.HasValue ||
                string.IsNullOrWhiteSpace(model.Ort))
            {
                return;
            }

            int jahr = model.Datum.Value.Year;

            if (jahr < 2000 || jahr > 2100)
            {
                return;
            }

            DateTime beginn = model.Datum.Value.Date +
                              model.Uhrzeit.Value.ToTimeSpan();

            string ort = model.Ort.Trim();

            // Beim Bearbeiten wird die aktuelle Trainingseinheit ausgeschlossen.
            bool trainingExistiert = await _db.Trainingseinheiten
                .AnyAsync(t =>
                    t.Id != model.Id &&
                    t.Beginn == beginn &&
                    t.Ort == ort);

            if (trainingExistiert)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"In \"{ort}\" ist am {beginn:dd.MM.yyyy} " +
                    $"um {beginn:HH:mm} Uhr bereits eine Trainingseinheit eingetragen."
                );
            }
        }
    }
}
