// ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using EMRSystem.Core.Entities;

namespace EMRSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<PasswordPolicy> PasswordPolicies { get; set; }
public DbSet<PasswordHistoryItem> PasswordHistory { get; set; }
public DbSet<DlpRule> DlpRules { get; set; }
public DbSet<DlpIncident> DlpIncidents { get; set; }
public DbSet<FileIntegrityRecord> FileIntegrityRecords { get; set; }
public DbSet<Vulnerability> Vulnerabilities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

             // Identity table names
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            // RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.Token).IsUnique();
            });

            // Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdentityCard).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LicenseNumber).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // MedicalRecord
            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.MedicalRecords)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Doctor)
                    .WithMany(d => d.MedicalRecords)
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Prescription
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.MedicalRecord)
                    .WithMany(m => m.Prescriptions)
                    .HasForeignKey(e => e.MedicalRecordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PrescriptionDetail
            modelBuilder.Entity<PrescriptionDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Prescription)
                    .WithMany(p => p.PrescriptionDetails)
                    .HasForeignKey(e => e.PrescriptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LabTest
            modelBuilder.Entity<LabTest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.MedicalRecord)
                    .WithMany(m => m.LabTests)
                    .HasForeignKey(e => e.MedicalRecordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}