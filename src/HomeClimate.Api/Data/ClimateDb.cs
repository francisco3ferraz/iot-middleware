using Microsoft.EntityFrameworkCore;

namespace HomeClimate.Api.Data;

public class ClimateDb(DbContextOptions<ClimateDb> options) : DbContext(options)
{
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<MetricKind> MetricKinds => Set<MetricKind>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetricKind>().HasKey(k => k.Kind);
        modelBuilder.Entity<MetricKind>().HasData(
            new MetricKind { Kind = "co2", Unit = "ppm", MinValue = 300, MaxValue = 10000 });

        modelBuilder.Entity<SensorMetric>().HasKey(m => new { m.SensorId, m.Kind });
        modelBuilder.Entity<SensorMetric>().HasOne<MetricKind>().WithMany().HasForeignKey(m => m.Kind);
    }
}
