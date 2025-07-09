using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace be.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Sensor> Sensors { get; set; }

    public virtual DbSet<SensorDatum> SensorData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=KUPHA;Database=QuetDuLieu;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sensor__3214EC0738AD46BC");

            entity.ToTable("Sensor");

            entity.HasIndex(e => e.SensorId, "UQ_Sensor_SensorId").IsUnique();

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SensorId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Unit)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SensorDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SensorDa__3214EC07406BC2E5");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SensorId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
            entity.Property(e => e.Unit)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Sensor).WithMany(p => p.SensorData)
                .HasPrincipalKey(p => p.SensorId)
                .HasForeignKey(d => d.SensorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SensorDat__Senso__47DBAE45");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
