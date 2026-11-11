using CoachBoard.Models;

namespace CoachBoard.ViewModels;

public class SpielerOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class TrainingIndexViewModel
{
    public List<Trainingseinheit> Items { get; set; } = new();

    // Paginierung
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    // aktuelle Filter
    public string? Monat { get; set; }                 // "2026-11"
    public string Zeitraum { get; set; } = "alle";     // alle | kommende | vergangene
    public string? Status { get; set; }                // Anwesend | Abwesend | Entschuldigt | Offen
    public int? SpielerId { get; set; }                // nur Trainer

    // Auswahl für den Trainer
    public List<SpielerOption> Spieler { get; set; } = new();

    // Status des Spielers (bei Spielern: eigener, bei Trainern: gewählter Spieler)
    public Dictionary<int, AnwesenheitsStatus> StatusProTraining { get; set; } = new();
    public string? StatusSpalte { get; set; }

    public bool HatFilter =>
        !string.IsNullOrEmpty(Monat) || Zeitraum != "alle" ||
        !string.IsNullOrEmpty(Status) || SpielerId.HasValue;
}

// Für die Seitenzahlen unter einer Liste
public class PagerViewModel
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string Controller { get; set; } = "Training";
    public string Action { get; set; } = "Index";
    public Dictionary<string, string> Route { get; set; } = new();   // aktive Filter
}