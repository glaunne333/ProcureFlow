using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProcureFlow.Infrastructure;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProcureFlowDbContext>
{
    public ProcureFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProcureFlowDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=procureflow;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new ProcureFlowDbContext(optionsBuilder.Options);
    }
}
