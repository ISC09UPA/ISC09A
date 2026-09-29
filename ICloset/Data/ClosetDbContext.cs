using Closet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Closet.Api.Data;

public class ClosetDbContext(DbContextOptions<ClosetDbContext> options) : DbContext(options)
{
    public DbSet<Garment> Garments => Set<Garment>();
}
