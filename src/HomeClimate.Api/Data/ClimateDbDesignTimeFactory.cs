using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HomeClimate.Api.Data;

// dotnet ef would otherwise run Program.cs, which needs Aspire's config; adding a migration never connects
public class ClimateDbDesignTimeFactory : IDesignTimeDbContextFactory<ClimateDb>
{
    public ClimateDb CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ClimateDb>().UseNpgsql().Options);
}
