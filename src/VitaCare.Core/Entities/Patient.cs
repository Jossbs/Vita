using System;
using System.Collections.Generic;
using VitaCare.Core.Enums;

namespace VitaCare.Core.Entities
{
    /// <summary>
    /// Represents the subject of care within the VITA ecosystem.
    /// </summary>
    public class Patient : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;

        public string? PreferredName { get; set; }

        public DateTime BirthDate { get; set; }

        // Examples: 'A+', 'O-', etc.
        public string? BloodType { get; set; }

        public CareLevel CareLevel { get; set; } = CareLevel.Basic;

        public MobilityStatus MobilityStatus { get; set; } = MobilityStatus.Independent;

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

        // Core of "Valoración Integral": Initial assessment of the patient's condition.
        public string? MedicalNotes { get; set; }

        // Traceability: Link to every health event registered for this patient.
        public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    }
}
