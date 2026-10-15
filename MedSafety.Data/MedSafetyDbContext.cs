using MedSafety.Data.Rows;
using Microsoft.EntityFrameworkCore;

namespace MedSafety.Data;

public class MedSafetyDbContext : DbContext
{
    public MedSafetyDbContext(DbContextOptions<MedSafetyDbContext> options) : base(options) { }

    public DbSet<DrugRow> Drugs => Set<DrugRow>();
    public DbSet<InteractionRow> Interactions => Set<InteractionRow>();
    public DbSet<PatientRow> Patients => Set<PatientRow>();
    public DbSet<AuditRow> AuditEntries => Set<AuditRow>();
    public DbSet<PrescriptionRow> Prescriptions => Set<PrescriptionRow>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Store enums as text so the database file is readable in the demo
        modelBuilder.Entity<DrugRow>().Property(d => d.DrugClass).HasConversion<string>();
        modelBuilder.Entity<DrugRow>().Property(d => d.DoseUnit).HasConversion<string>();
        modelBuilder.Entity<InteractionRow>().Property(i => i.Severity).HasConversion<string>();
        modelBuilder.Entity<AuditRow>().Property(a => a.Role).HasConversion<string>();
        modelBuilder.Entity<PrescriptionRow>().Property(p => p.DoseUnit).HasConversion<string>();
        modelBuilder.Entity<PrescriptionRow>().Property(p => p.Outcome).HasConversion<string>();

    }
}