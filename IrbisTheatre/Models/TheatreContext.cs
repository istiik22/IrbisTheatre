using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Reflection.Emit;

namespace IrbisTheatre.Models;

public class TheatreContext : DbContext
{
    public TheatreContext(DbContextOptions<TheatreContext> options)
        : base(options)
    {
        
    }

    // DbSet для каждой таблицы
    public DbSet<Author> Authors { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Play> Plays { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Performance> Performances { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Employer> Employers { get; set; }
    public DbSet<ProductionTeam> ProductionTeams { get; set; }
    public DbSet<PerformanceGroup> PerformanceGroups { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<EmployerProperty> EmployerProperties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка уникальных ключей и ограничений

        // Genre: name уникально
        modelBuilder.Entity<Genre>()
            .HasIndex(g => g.Name)
            .IsUnique();

        // Property: name уникально
        modelBuilder.Entity<Property>()
            .HasIndex(p => p.Name)
            .IsUnique();

        // Seat: уникальная комбинация (hall_id, row_number, seat_number)
        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.HallId, s.RowNumber, s.SeatNumber })
            .IsUnique();

        // Ticket: уникальный номер
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.UniqueNumber)
            .IsUnique();

        // Настройка enum полей
        modelBuilder.Entity<Role>()
            .Property(r => r.GenderRequirement)
            .HasDefaultValue("any");

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Status)
            .HasDefaultValue("free");

        modelBuilder.Entity<Employer>()
            .Property(e => e.Status)
            .HasDefaultValue("работает");

        modelBuilder.Entity<Performance>()
            .Property(p => p.IsPremiere)
            .HasDefaultValue(false);

        // Настройка связей (Foreign Keys)

        // Play -> Author
        modelBuilder.Entity<Play>()
            .HasOne(p => p.Author)
            .WithMany(a => a.Plays)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Play -> Genre
        modelBuilder.Entity<Play>()
            .HasOne(p => p.Genre)
            .WithMany(g => g.Plays)
            .HasForeignKey(p => p.GenreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Role -> Play
        modelBuilder.Entity<Role>()
            .HasOne(r => r.Play)
            .WithMany(p => p.Roles)
            .HasForeignKey(r => r.PlayId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seat -> Hall
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Hall)
            .WithMany(h => h.Seats)
            .HasForeignKey(s => s.HallId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance -> Play
        modelBuilder.Entity<Performance>()
            .HasOne(p => p.Play)
            .WithMany(pl => pl.Performances)
            .HasForeignKey(p => p.PlayId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket -> Performance
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Performance)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PerformanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ticket -> Seat
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Seat)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProductionTeam -> Employer
        modelBuilder.Entity<ProductionTeam>()
            .HasOne(pt => pt.Employer)
            .WithMany(e => e.ProductionTeams)
            .HasForeignKey(pt => pt.EmployerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProductionTeam -> Role
        modelBuilder.Entity<ProductionTeam>()
            .HasOne(pt => pt.Role)
            .WithMany(r => r.ProductionTeams)
            .HasForeignKey(pt => pt.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // PerformanceGroup -> Performance
        modelBuilder.Entity<PerformanceGroup>()
            .HasOne(pg => pg.Performance)
            .WithMany(p => p.PerformanceGroups)
            .HasForeignKey(pg => pg.PerformanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // PerformanceGroup -> ProductionTeam
        modelBuilder.Entity<PerformanceGroup>()
            .HasOne(pg => pg.ProductionTeam)
            .WithMany(pt => pt.PerformanceGroups)
            .HasForeignKey(pg => pg.ProductionTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // EmployerProperty -> Employer
        modelBuilder.Entity<EmployerProperty>()
            .HasOne(ep => ep.Employer)
            .WithMany(e => e.EmployerProperties)
            .HasForeignKey(ep => ep.EmployerId)
            .OnDelete(DeleteBehavior.Cascade);

        // EmployerProperty -> Property
        modelBuilder.Entity<EmployerProperty>()
            .HasOne(ep => ep.Property)
            .WithMany(p => p.EmployerProperties)
            .HasForeignKey(ep => ep.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}