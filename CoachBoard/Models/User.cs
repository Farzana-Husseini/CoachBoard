using System.ComponentModel.DataAnnotations;

namespace CoachBoard.Models
{
    public static class AppRoles
    {
        public const string Trainer = "Trainer";
        public const string Spieler = "Spieler";
    }

    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Vorname { get; set; } = "";

        [Required, StringLength(100)]
        public string Nachname { get; set; } = "";

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [Required, StringLength(20)]
        public string Role { get; set; } = AppRoles.Spieler;

        public string VollerName => $"{Vorname} {Nachname}";
    }
}
