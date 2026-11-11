using System.ComponentModel.DataAnnotations;

namespace CoachBoard.ViewModels;

public class TrainingFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte ein Datum angeben.")]
    [DataType(DataType.Date)]
    [Display(Name = "Datum")]
    public DateTime? Datum { get; set; }

    [Required(ErrorMessage = "Bitte eine Uhrzeit angeben.")]
    [DataType(DataType.Time)]
    [DisplayFormat(DataFormatString = "{0:HH\\:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Uhrzeit")]
    public TimeOnly? Uhrzeit { get; set; }

    [Required(ErrorMessage = "Bitte Ort bzw. Halle angeben.")]
    [StringLength(200, ErrorMessage = "Der Ort darf höchstens 200 Zeichen lang sein.")]
    [Display(Name = "Ort / Halle")]
    public string Ort { get; set; } = "";

    [StringLength(1000, ErrorMessage = "Die Beschreibung darf höchstens 1000 Zeichen lang sein.")]
    [Display(Name = "Beschreibung (Schwerpunkt)")]
    public string? Beschreibung { get; set; }

    [StringLength(4000, ErrorMessage = "Die Übungen dürfen höchstens 4000 Zeichen lang sein.")]
    [Display(Name = "Geplante Übungen")]
    public string? GeplanteUebungen { get; set; }
}