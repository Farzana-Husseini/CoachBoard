using CoachBoard.Models;

namespace CoachBoard.ViewModels;

// --- Liste pro Trainingseinheit (Trainer) ---
public class AnwesenheitZeile
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public AnwesenheitsStatus? Status { get; set; }     // null = noch keine Rückmeldung
    public DateTime? GeaendertAm { get; set; }
}

public class EinheitAnwesenheitViewModel
{
    public Trainingseinheit Training { get; set; } = null!;
    public List<AnwesenheitZeile> Zeilen { get; set; } = new();

    public int Anwesend => Zeilen.Count(z => z.Status == AnwesenheitsStatus.Anwesend);
    public int Abwesend => Zeilen.Count(z => z.Status == AnwesenheitsStatus.Abwesend);
    public int Entschuldigt => Zeilen.Count(z => z.Status == AnwesenheitsStatus.Entschuldigt);
    public int Offen => Zeilen.Count(z => z.Status == null);
}

// --- Quote pro Spieler (Trainer) ---
public class QuoteZeile
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public int Anwesend { get; set; }
    public int Abwesend { get; set; }
    public int Entschuldigt { get; set; }
    public int Offen { get; set; }
    public int Gesamt { get; set; }

    // Anzahl anwesend / Anzahl Trainings gesamt in Prozent
    public double Quote => Gesamt == 0 ? 0 : Math.Round(Anwesend * 100.0 / Gesamt, 1);
}

public class QuoteViewModel
{
    public List<QuoteZeile> Zeilen { get; set; } = new();
    public int AnzahlTrainings { get; set; }
    public bool NurStattgefundene { get; set; }
}
// --- Persönliche Historie (Spieler) ---
public class HistorieZeile
{
    public Trainingseinheit Training { get; set; } = null!;
    public AnwesenheitsStatus? Status { get; set; }     // null = keine Rückmeldung
}

public class MeineHistorieViewModel
{
    public List<HistorieZeile> Zeilen { get; set; } = new();

    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public int Gesamt { get; set; }
    public int Anwesend { get; set; }
    public int Abwesend { get; set; }
    public int Entschuldigt { get; set; }
    public int Offen => Gesamt - Anwesend - Abwesend - Entschuldigt;

    public double Quote => Gesamt == 0 ? 0 : Math.Round(Anwesend * 100.0 / Gesamt, 1);
    public bool NurStattgefundene { get; set; }
}