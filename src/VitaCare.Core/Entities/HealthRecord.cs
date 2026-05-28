using VitaCare.Core.Enums;

namespace VitaCare.Core.Entities
{
    public class HealthRecord : BaseEntity
    {
        // Links the record to a specific Patient.
        public Guid PatientId { get; set; }
        public Patient? Patient { get; set; }

        // Links the record to the User (Caregiver) who performed the action.
        public Guid CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        // Uses our Enum to classify the event.
        public RecordType Category { get; set; }

        // This stores the specific data (e.g., Blood Pressure: 120/80) as a JSON string.
        public string JsonData { get; set; } = string.Empty;

        public string? Observations { get; set; }
    }
}