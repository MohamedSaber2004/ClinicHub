using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddPatientAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssessmentDate = table.Column<DateTime>(type: "date", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Handedness = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialHabits = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HeightCm = table.Column<double>(type: "float", nullable: true),
                    WeightKg = table.Column<double>(type: "float", nullable: true),
                    Bmi = table.Column<double>(type: "float", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OnsetOfSymptoms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DurationOfSymptoms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CourseOfSymptoms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MechanismOfInjury = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiagnosticImagingAndLab = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PreviousTreatment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DateOfAdmission = table.Column<DateTime>(type: "date", nullable: true),
                    DateAndTypeOfSurgery = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SpecialPrecautions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MedicalHistory = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FamilyHistory = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Medications = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PhysicalTherapy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TreatmentDuration = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FollowUpNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientAssessments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PatientAssessments_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PatientAssessments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientAssessments_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientAssessments_AppointmentId",
                table: "PatientAssessments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAssessments_AssessmentDate",
                table: "PatientAssessments",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAssessments_ClinicId",
                table: "PatientAssessments",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAssessments_DoctorId",
                table: "PatientAssessments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAssessments_PatientId",
                table: "PatientAssessments",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientAssessments");
        }
    }
}
