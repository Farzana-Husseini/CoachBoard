using System.ComponentModel.DataAnnotations;

namespace CoachBoard.ViewModels;

public class SpielerCreateViewModel
{
    [Required, StringLength(100)] public string Vorname { get; set; } = "";
    [Required, StringLength(100)] public string Nachname { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";

    [Required, DataType(DataType.Password), MinLength(8, ErrorMessage = "Mindestens 8 Zeichen")]
    public string Password { get; set; } = "";

    [Required]
    [Display(Name = "Rolle")]
    public string Role { get; set; } = "Spieler";
}

public class SpielerEditViewModel
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Vorname { get; set; } = "";
    [Required, StringLength(100)] public string Nachname { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";

    [DataType(DataType.Password), MinLength(8, ErrorMessage = "Mindestens 8 Zeichen")]
    [Display(Name = "Neues Passwort (leer lassen = unverändert)")]
    public string? NewPassword { get; set; }

    [Required]
    [Display(Name = "Rolle")]
    public string Role { get; set; } = "Spieler";
}