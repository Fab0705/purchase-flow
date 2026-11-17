using backend.Modules.FieldService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.FieldService.Database
{
    public sealed class FieldServiceDbContext : DbContext
    {
        public FieldServiceDbContext(DbContextOptions<FieldServiceDbContext> options) : base(options)
        {
        }
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("fieldservice");

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Tickets");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title).IsRequired().HasMaxLength(150);
                entity.Property(t => t.Description).HasMaxLength(1000);

                // Enum a String
                entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

                // Referencias Externas (Módulo Identity e Inventory) indexadas sin Foreign Keys
                entity.Property(t => t.LocationId).IsRequired();
                entity.HasIndex(t => t.LocationId);

                entity.Property(t => t.AssignedTechnicianId).IsRequired(false);
                entity.HasIndex(t => t.AssignedTechnicianId);

                // Campos heredados de AggregateRoot (Auditoría)
                entity.Property(t => t.CreatedAtUtc).IsRequired();
                entity.Property(t => t.CreatedBy).HasMaxLength(100);
                entity.Property(t => t.LastModifiedAtUtc);
                entity.Property(t => t.LastModifiedBy).HasMaxLength(100);
            });
        }
    }
}