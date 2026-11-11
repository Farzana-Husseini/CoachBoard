using System.ComponentModel.DataAnnotations;

namespace CoachBoard.Models;

public class Trainingseinheit
{
    public int Id { get; set; }

    public DateTime Beginn { get; set; }          // Datum + Uhrzeit zusammen

    [Required, StringLength(200)]
    public string Ort { get; set; } = "";

    [StringLength(1000)]
    public string? Beschreibung { get; set; }

    [StringLength(4000)]
    public string? GeplanteUebungen { get; set; }
}