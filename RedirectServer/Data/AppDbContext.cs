using Microsoft.EntityFrameworkCore;
using RedirectServer.Models;

namespace RedirectServer.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortLink> ShortLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortLink>()
            .HasIndex(s => s.ShortCode)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}