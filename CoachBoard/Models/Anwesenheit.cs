namespace CoachBoard.Models;

public enum AnwesenheitsStatus
{
    Anwesend = 1,
    Abwesend = 2,
    Entschuldigt = 3
}

public class Anwesenheit
{
    public int Id { get; set; }

    public int TrainingseinheitId { get; set; }
    public Trainingseinheit Trainingseinheit { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public AnwesenheitsStatus Status { get; set; }

    public DateTime GeaendertAm { get; set; } = DateTime.Now;
}

// Farben für die Status-Anzeige (Bootstrap-Badges)
public static class AnwesenheitsAnzeige
{
    public static string Badge(AnwesenheitsStatus s) => s switch
    {
        AnwesenheitsStatus.Anwesend => "bg-success",
        AnwesenheitsStatus.Abwesend => "bg-danger",
        AnwesenheitsStatus.Entschuldigt => "bg-warning text-dark",
        _ => "bg-secondary"
    };
}