using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration
{
    public class PatientAssessmentConfiguration : IEntityTypeConfiguration<PatientAssessment>
    {
        public void Configure(EntityTypeBuilder<PatientAssessment> builder)
        {
            builder.ToTable("PatientAssessments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AssessmentDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.PatientName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.Occupation).HasMaxLength(200);
            builder.Property(x => x.Handedness).HasMaxLength(50);
            builder.Property(x => x.SpecialHabits).HasMaxLength(500);
            builder.Property(x => x.ContactNumber).HasMaxLength(50);
            builder.Property(x => x.Address).HasMaxLength(300);

            builder.Property(x => x.Diagnosis).HasMaxLength(1000);
            builder.Property(x => x.ChiefComplaint).HasMaxLength(1000);

            builder.Property(x => x.OnsetOfSymptoms).HasMaxLength(200);
            builder.Property(x => x.DurationOfSymptoms).HasMaxLength(200);
            builder.Property(x => x.CourseOfSymptoms).HasMaxLength(200);
            builder.Property(x => x.MechanismOfInjury).HasMaxLength(500);
            builder.Property(x => x.DiagnosticImagingAndLab).HasMaxLength(1000);
            builder.Property(x => x.PreviousTreatment).HasMaxLength(1000);
            builder.Property(x => x.DateOfAdmission).HasColumnType("date");
            builder.Property(x => x.DateAndTypeOfSurgery).HasMaxLength(500);
            builder.Property(x => x.SpecialPrecautions).HasMaxLength(1000);

            builder.Property(x => x.MedicalHistory).HasMaxLength(2000);
            builder.Property(x => x.FamilyHistory).HasMaxLength(2000);

            builder.Property(x => x.TreatmentPlan).HasMaxLength(2000);
            builder.Property(x => x.Medications).HasMaxLength(2000);
            builder.Property(x => x.PhysicalTherapy).HasMaxLength(2000);
            builder.Property(x => x.TreatmentDuration).HasMaxLength(200);
            builder.Property(x => x.FollowUpNotes).HasMaxLength(2000);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Appointment)
                .WithMany()
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.DoctorId);
            builder.HasIndex(x => x.AssessmentDate);
        }
    }
}
