using System.ComponentModel.DataAnnotations;
using VitaCare.Core.Enums;

namespace VitaCare.Api.Contracts.Patients
{
    public class PatientRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string? FullName { get; set; }

        [StringLength(100)]
        public string? PreferredName { get; set; }

        [Required]
        public DateTime? BirthDate { get; set; }

        [StringLength(5)]
        public string? BloodType { get; set; }

        public CareLevel CareLevel { get; set; } = CareLevel.Basic;

        public MobilityStatus MobilityStatus { get; set; } = MobilityStatus.Independent;

        public bool RequiresContinuousSupervision { get; set; }

        public bool RequiresMedicationAssistance { get; set; }

        public bool RequiresFeedingAssistance { get; set; }

        [StringLength(200)]
        public string? PrimaryCondition { get; set; }

        [StringLength(1000)]
        public string? Allergies { get; set; }

        [StringLength(1000)]
        public string? CurrentMedications { get; set; }

        [StringLength(2000)]
        public string? CareInstructions { get; set; }

        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        [StringLength(50)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(100)]
        public string? EmergencyContactRelationship { get; set; }

        [StringLength(2000)]
        public string? MedicalNotes { get; set; }
    }
}
