using VitaCare.Core.Enums;

namespace VitaCare.Api.Contracts.Patients
{
    public class PatientResponse
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        // Convenience display name composed from FirstName + LastName.
        public string FullName { get; set; } = string.Empty;

        public string? PreferredName { get; set; }

        public DateTime BirthDate { get; set; }

        public string? BloodType { get; set; }

        public CareLevel CareLevel { get; set; }

        public MobilityStatus MobilityStatus { get; set; }

        public bool RequiresContinuousSupervision { get; set; }

        public bool RequiresMedicationAssistance { get; set; }

        public bool RequiresFeedingAssistance { get; set; }

        public string? PrimaryCondition { get; set; }

        public string? Allergies { get; set; }

        public string? CurrentMedications { get; set; }

        public string? CareInstructions { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? EmergencyContactRelationship { get; set; }

        public string? MedicalNotes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
