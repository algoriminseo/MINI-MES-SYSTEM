using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Models;

namespace MiniMES.Api.Data;

public class MiniMesDbContext(DbContextOptions<MiniMesDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.HasIndex(item => item.ItemCode)
                .IsUnique();

            entity.Property(item => item.ItemCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(item => item.ItemName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(item => item.Specification)
                .HasMaxLength(200);

            entity.Property(item => item.Unit)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(item => item.ItemType)
                .HasMaxLength(30)
                .IsRequired();
        });
    }
}
