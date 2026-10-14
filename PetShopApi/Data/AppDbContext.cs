using Microsoft.EntityFrameworkCore;
using PetShopApi.Models;

namespace PetShopApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tutor>(t =>
        {
            t.Property(x => x.Nome).IsRequired().HasMaxLength(120);
            t.Property(x => x.Email).IsRequired().HasMaxLength(150);
            t.Property(x => x.Telefone).IsRequired().HasMaxLength(20);
            t.HasIndex(x => x.Email).IsUnique();
            t.HasMany(x => x.Pets)
             .WithOne(p => p.Tutor)
             .HasForeignKey(p => p.TutorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pet>(p =>
        {
            p.Property(x => x.Nome).IsRequired().HasMaxLength(80);
            p.Property(x => x.Especie).IsRequired().HasMaxLength(50);
            p.Property(x => x.Raca).HasMaxLength(80);
            p.HasMany(x => x.Agendamentos)
             .WithOne(a => a.Pet)
             .HasForeignKey(a => a.PetId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Agendamento>(a =>
        {
            a.Property(x => x.Valor).HasPrecision(10, 2);
            a.Property(x => x.Servico).HasConversion<string>().HasMaxLength(20);
            a.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
