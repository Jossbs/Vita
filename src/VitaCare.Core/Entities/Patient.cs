using System;
using System.Collections.Generic;

namespace VitaCare.Core.Entities
{
    /// <summary>
    /// Represents the subject of care within the VITA ecosystem.
    /// </summary>
    public class Patient : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        // Examples: 'A+', 'O-', etc.
        public string? BloodType { get; set; }

        // Core of "Valoración Integral": Initial assessment of the patient's condition.
        public string? MedicalNotes { get; set; }

        // Traceability: Link to every health event registered for this patient.
        public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    }
}