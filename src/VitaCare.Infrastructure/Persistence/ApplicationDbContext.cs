using Microsoft.EntityFrameworkCore;
using VitaCare.Core.Entities;

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
            modelBuilder.Entity<Patient>(entity => {
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(200);
                entity.Property(p => p.BloodType).HasMaxLength(5);
            });

            modelBuilder.Entity<User>(entity => {
                entity.HasIndex(u => u.Email).IsUnique(); // Performance and security
            });

            // Mapping JSONB for PostgreSQL
            modelBuilder.Entity<HealthRecord>()
                .Property(b => b.JsonData)
                .HasColumnType("jsonb");
        }
    }
}