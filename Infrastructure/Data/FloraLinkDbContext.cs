using FloraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FloraLink.Infrastructure.Data;

public class FloraLinkDbContext : DbContext
{
    public FloraLinkDbContext(DbContextOptions<FloraLinkDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<PlantType> PlantTypes => Set<PlantType>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<WateringEvent> WateringEvents => Set<WateringEvent>();
    public DbSet<PlantDiaryEntry> PlantDiaryEntries => Set<PlantDiaryEntry>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        // Plant → User
        modelBuilder.Entity<Plant>()
            .HasOne(p => p.User)
            .WithMany(u => u.Plants)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plant → PlantType
        modelBuilder.Entity<Plant>()
            .HasOne(p => p.PlantType)
            .WithMany(pt => pt.Plants)
            .HasForeignKey(p => p.PlantTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // SensorReading → Plant
        modelBuilder.Entity<SensorReading>()
            .HasOne(r => r.Plant)
            .WithMany(p => p.SensorReadings)
            .HasForeignKey(r => r.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // WateringEvent → Plant
        modelBuilder.Entity<WateringEvent>()
            .HasOne(w => w.Plant)
            .WithMany(p => p.WateringEvents)
            .HasForeignKey(w => w.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlantDiaryEntry → Plant
        modelBuilder.Entity<PlantDiaryEntry>()
            .HasOne(d => d.Plant)
            .WithMany(p => p.DiaryEntries)
            .HasForeignKey(d => d.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Alert → Plant
        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Plant)
            .WithMany(p => p.Alerts)
            .HasForeignKey(a => a.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
