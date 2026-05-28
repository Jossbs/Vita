using Microsoft.EntityFrameworkCore;
using VitaCare.Api.Contracts.Patients;
using VitaCare.Core.Entities;
using VitaCare.Infrastructure.Persistence;

namespace VitaCare.Api.Services.Patients
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _dbContext;

        public PatientService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Patients
                .AsNoTracking()
                .OrderBy(patient => patient.FullName)
                .Select(patient => new PatientResponse
                {
                    Id = patient.Id,
                    FullName = patient.FullName,
                    PreferredName = patient.PreferredName,
                    BirthDate = patient.BirthDate,
                    BloodType = patient.BloodType,
                    CareLevel = patient.CareLevel,
                    MobilityStatus = patient.MobilityStatus,
                    RequiresContinuousSupervision = patient.RequiresContinuousSupervision,
                    RequiresMedicationAssistance = patient.RequiresMedicationAssistance,
                    RequiresFeedingAssistance = patient.RequiresFeedingAssistance,
                    PrimaryCondition = patient.PrimaryCondition,
                    Allergies = patient.Allergies,
                    CurrentMedications = patient.CurrentMedications,
                    CareInstructions = patient.CareInstructions,
                    EmergencyContactName = patient.EmergencyContactName,
                    EmergencyContactPhone = patient.EmergencyContactPhone,
                    EmergencyContactRelationship = patient.EmergencyContactRelationship,
                    MedicalNotes = patient.MedicalNotes,
                    CreatedAt = patient.CreatedAt,
                    UpdatedAt = patient.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var patient = await _dbContext.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(patient => patient.Id == id, cancellationToken);

            return patient is null ? null : ToResponse(patient);
        }

        public async Task<PatientResponse> CreateAsync(PatientRequest request, CancellationToken cancellationToken = default)
        {
            var patient = new Patient();
            ApplyRequest(patient, request);

            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ToResponse(patient);
        }

        public async Task<PatientResponse?> UpdateAsync(
            Guid id,
            PatientRequest request,
            CancellationToken cancellationToken = default)
        {
            var patient = await _dbContext.Patients.FindAsync([id], cancellationToken);

            if (patient is null)
            {
                return null;
            }

            ApplyRequest(patient, request);
            patient.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ToResponse(patient);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var patient = await _dbContext.Patients.FindAsync([id], cancellationToken);

            if (patient is null)
            {
                return false;
            }

            _dbContext.Patients.Remove(patient);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static void ApplyRequest(Patient patient, PatientRequest request)
        {
            patient.FullName = request.FullName!.Trim();
            patient.PreferredName = Normalize(request.PreferredName);
            patient.BirthDate = request.BirthDate!.Value;
            patient.BloodType = Normalize(request.BloodType);
            patient.CareLevel = request.CareLevel;
            patient.MobilityStatus = request.MobilityStatus;
            patient.RequiresContinuousSupervision = request.RequiresContinuousSupervision;
            patient.RequiresMedicationAssistance = request.RequiresMedicationAssistance;
            patient.RequiresFeedingAssistance = request.RequiresFeedingAssistance;
            patient.PrimaryCondition = Normalize(request.PrimaryCondition);
            patient.Allergies = Normalize(request.Allergies);
            patient.CurrentMedications = Normalize(request.CurrentMedications);
            patient.CareInstructions = Normalize(request.CareInstructions);
            patient.EmergencyContactName = Normalize(request.EmergencyContactName);
            patient.EmergencyContactPhone = Normalize(request.EmergencyContactPhone);
            patient.EmergencyContactRelationship = Normalize(request.EmergencyContactRelationship);
            patient.MedicalNotes = Normalize(request.MedicalNotes);
        }

        private static PatientResponse ToResponse(Patient patient)
        {
            return new PatientResponse
            {
                Id = patient.Id,
                FullName = patient.FullName,
                PreferredName = patient.PreferredName,
                BirthDate = patient.BirthDate,
                BloodType = patient.BloodType,
                CareLevel = patient.CareLevel,
                MobilityStatus = patient.MobilityStatus,
                RequiresContinuousSupervision = patient.RequiresContinuousSupervision,
                RequiresMedicationAssistance = patient.RequiresMedicationAssistance,
                RequiresFeedingAssistance = patient.RequiresFeedingAssistance,
                PrimaryCondition = patient.PrimaryCondition,
                Allergies = patient.Allergies,
                CurrentMedications = patient.CurrentMedications,
                CareInstructions = patient.CareInstructions,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                EmergencyContactRelationship = patient.EmergencyContactRelationship,
                MedicalNotes = patient.MedicalNotes,
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
