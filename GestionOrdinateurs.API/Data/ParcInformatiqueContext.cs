using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Data;

public class ParcInformatiqueContext : DbContext
{
    public ParcInformatiqueContext() { }

    public ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options) 
        : base(options) { }

    //La table Ordinateurs
    public DbSet<Ordinateur> Ordinateurs { get; set; }

    //La table Reparations
    public DbSet<Reparation> Reparations { get; set; }

    //La table Users
    public DbSet<User> Users { get; set; }

    //Configuration de SQLite
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite("Data Source=parcinfo.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // les caractéristiques sont stockées dans les colonnes de la table Ordinateur
        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(o => o.Caracteristiques);

        // un ordinateur a plusieurs réparations, OrdinateurId est la clé étrangère
        modelBuilder.Entity<Ordinateur>()
            .HasMany(o => o.Reparations)
            .WithOne(r => r.UnOrdinateur)
            .HasForeignKey(r => r.OrdinateurId);

        // le login d'un utilisateur doit être unique en base (index unique)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();
    }
}
