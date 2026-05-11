using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Models;

namespace MiniMES.Api.Data;

public class MiniMesDbContext(DbContextOptions<MiniMesDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    public DbSet<ManufacturingProcess> Processes => Set<ManufacturingProcess>();

    public DbSet<Equipment> Equipments => Set<Equipment>();

    public DbSet<Worker> Workers => Set<Worker>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

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

        modelBuilder.Entity<ManufacturingProcess>(entity =>
        {
            entity.HasKey(process => process.Id);

            entity.HasIndex(process => process.ProcessCode)
                .IsUnique();

            entity.Property(process => process.ProcessCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(process => process.ProcessName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(process => process.Description)
                .HasMaxLength(300);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(equipment => equipment.Id);

            entity.HasIndex(equipment => equipment.EquipmentCode)
                .IsUnique();

            entity.Property(equipment => equipment.EquipmentCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(equipment => equipment.EquipmentName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(equipment => equipment.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(equipment => equipment.Location)
                .HasMaxLength(100);

            entity.Property(equipment => equipment.Description)
                .HasMaxLength(300);

            entity.HasOne(equipment => equipment.Process)
                .WithMany()
                .HasForeignKey(equipment => equipment.ProcessId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(worker => worker.Id);

            entity.HasIndex(worker => worker.WorkerCode)
                .IsUnique();

            entity.Property(worker => worker.WorkerCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(worker => worker.WorkerName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(worker => worker.Department)
                .HasMaxLength(100);

            entity.Property(worker => worker.Role)
                .HasMaxLength(50);

            entity.Property(worker => worker.ShiftGroup)
                .HasMaxLength(30);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(workOrder => workOrder.Id);

            entity.HasIndex(workOrder => workOrder.WorkOrderNo)
                .IsUnique();

            entity.Property(workOrder => workOrder.WorkOrderNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(workOrder => workOrder.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(workOrder => workOrder.Remark)
                .HasMaxLength(500);

            entity.HasOne(workOrder => workOrder.Item)
                .WithMany()
                .HasForeignKey(workOrder => workOrder.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
