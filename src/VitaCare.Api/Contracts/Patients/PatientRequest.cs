using System.ComponentModel.DataAnnotations;
using VitaCare.Core.Enums;

namespace VitaCare.Api.Contracts.Patients
{
    public class PatientRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
        public string? LastName { get; set; }

        [StringLength(100, ErrorMessage = "El alias no puede superar los 100 caracteres.")]
        public string? PreferredName { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime? BirthDate { get; set; }

        [StringLength(5, ErrorMessage = "El tipo de sangre no puede superar los 5 caracteres.")]
        public string? BloodType { get; set; }

        public CareLevel CareLevel { get; set; } = CareLevel.Basic;

        public MobilityStatus MobilityStatus { get; set; } = MobilityStatus.Independent;

        public bool RequiresContinuousSupervision { get; set; }

        public bool RequiresMedicationAssistance { get; set; }

        public bool RequiresFeedingAssistance { get; set; }

        [StringLength(200, ErrorMessage = "La condición principal no puede superar los 200 caracteres.")]
        public string? PrimaryCondition { get; set; }

        [StringLength(1000, ErrorMessage = "Las alergias no pueden superar los 1000 caracteres.")]
        public string? Allergies { get; set; }

        [StringLength(1000, ErrorMessage = "Los medicamentos actuales no pueden superar los 1000 caracteres.")]
        public string? CurrentMedications { get; set; }

        [StringLength(2000, ErrorMessage = "Las instrucciones de cuidado no pueden superar los 2000 caracteres.")]
        public string? CareInstructions { get; set; }

        [StringLength(200, ErrorMessage = "El nombre del contacto de emergencia no puede superar los 200 caracteres.")]
        public string? EmergencyContactName { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono del contacto de emergencia no puede superar los 50 caracteres.")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(100, ErrorMessage = "La relación del contacto de emergencia no puede superar los 100 caracteres.")]
        public string? EmergencyContactRelationship { get; set; }

        [StringLength(2000, ErrorMessage = "Las notas médicas no pueden superar los 2000 caracteres.")]
        public string? MedicalNotes { get; set; }
    }
}
