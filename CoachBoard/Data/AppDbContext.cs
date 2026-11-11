using Microsoft.EntityFrameworkCore;
using CoachBoard.Models;

namespace CoachBoard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Trainingseinheit> Trainingseinheiten => Set<Trainingseinheit>();
    public DbSet<Anwesenheit> Anwesenheiten => Set<Anwesenheit>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        
        b.Entity<Anwesenheit>(e =>
        {
            // pro Trainingseinheit und Spieler nur ein Eintrag
            e.HasIndex(a => new { a.TrainingseinheitId, a.UserId }).IsUnique();

            // Status lesbar als Text in der Datenbank speichern
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(a => a.Trainingseinheit).WithMany()
                .HasForeignKey(a => a.TrainingseinheitId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.User).WithMany()
                .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}