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
        LoadDotEnv(Directory.GetCurrentDirectory());
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=reportu;Username=reportu;Password=reportu_dev";
        var options = new DbContextOptionsBuilder<ReportUDbContext>()
            .UseNpgsql(cs)
            .Options;
        return new ReportUDbContext(options);
    }

    private static void LoadDotEnv(string contentRoot)
    {
        var path = Path.Combine(contentRoot, ".env");
        if (!File.Exists(path)) return;

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var separator = line.IndexOf('=');
            if (separator <= 0) continue;

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"').Trim('\'');
            if (!string.IsNullOrEmpty(key) && Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
