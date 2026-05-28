using Microsoft.EntityFrameworkCore;
using VitaCare.Core.Entities;
using VitaCare.Core.Enums;

namespace VitaCare.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Mapping our Entities to Database Tables
        public DbSet<Patient> Patients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Strict Typing and Constraints
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(200);
                entity.Property(p => p.PreferredName).HasMaxLength(100);
                entity.Property(p => p.BloodType).HasMaxLength(5);
                entity.Property(p => p.PrimaryCondition).HasMaxLength(200);
                entity.Property(p => p.Allergies).HasMaxLength(1000);
                entity.Property(p => p.CurrentMedications).HasMaxLength(1000);
                entity.Property(p => p.CareInstructions).HasMaxLength(2000);
                entity.Property(p => p.EmergencyContactName).HasMaxLength(200);
                entity.Property(p => p.EmergencyContactPhone).HasMaxLength(50);
                entity.Property(p => p.EmergencyContactRelationship).HasMaxLength(100);
                entity.Property(p => p.MedicalNotes).HasMaxLength(2000);

                entity.Property(p => p.CareLevel)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasDefaultValue(CareLevel.Basic);

                entity.Property(p => p.MobilityStatus)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasDefaultValue(MobilityStatus.Independent);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique(); // Performance and security
            });

            // Mapping JSONB for PostgreSQL
            modelBuilder.Entity<HealthRecord>()
                .Property(b => b.JsonData)
                .HasColumnType("jsonb");
        }
    }
}
