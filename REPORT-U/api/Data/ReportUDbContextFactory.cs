using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReportU.Data;

/// <summary>
/// Factoría solo para tiempo de diseño (`dotnet ef migrations ...`).
/// Lee la misma variable de entorno que la app y cae a la BD local del compose.
/// </summary>
public class ReportUDbContextFactory : IDesignTimeDbContextFactory<ReportUDbContext>
{
    public ReportUDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=reportu;Username=reportu;Password=reportu_dev";
        var options = new DbContextOptionsBuilder<ReportUDbContext>()
            .UseNpgsql(cs)
            .Options;
        return new ReportUDbContext(options);
    }
}
