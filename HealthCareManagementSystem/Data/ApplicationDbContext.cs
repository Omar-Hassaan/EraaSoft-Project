using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<MedicalFile> MedicalFiles { get; set; }
        public DbSet<Clinic> Clinics { get; set; }

        public DbSet<ClinicPatient> ClinicPatients { get; set; }
        public DbSet<PatientDoctor> PatientDoctors { get; set; }
        public DbSet<Receptionist> Receptionists { get; set; }

        // AddModelApplicationUserOtp
        public DbSet<ApplicationUserOtp> ApplicationUserOtps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PatientDoctor>()
                .HasKey(x => new { x.PatientId, x.DoctorId });

            modelBuilder.Entity<PatientDoctor>()
                .HasOne(x => x.Patient)
                .WithMany(p => p.PatientDoctors)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PatientDoctor>()
                .HasOne(x => x.Doctor)
                .WithMany(d => d.PatientDoctors)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClinicPatient>()
                .HasKey(x => new { x.ClinicId, x.PatientId });

            modelBuilder.Entity<ClinicPatient>()
                .HasOne(x => x.Clinic)
                .WithMany(c => c.ClinicPatients)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClinicPatient>()
                .HasOne(x => x.Patient)
                .WithMany(p => p.ClinicPatients)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DoctorSchedule>()
                .HasOne(s => s.Doctor)
                .WithMany(d => d.DoctorSchedules)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Appointment)
                .WithMany(a => a.MedicalRecords)
                .HasForeignKey(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.MedicalRecord)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(p => p.MedicalRecordId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MedicalFile>()
                .HasOne(f => f.Patient)
                .WithMany(p => p.MedicalFiles)
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Receptionist>()
                .HasOne(r => r.ApplicationUser)
                .WithMany()
                .HasForeignKey(r => r.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            }
        }
    }
