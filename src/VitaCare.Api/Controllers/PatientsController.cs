using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaCare.Api.Contracts.Patients;
using VitaCare.Core.Entities;
using VitaCare.Infrastructure.Persistence;

namespace VitaCare.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PatientsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PatientResponse>>> GetAll()
        {
            var patients = await _dbContext.Patients
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
                .ToListAsync();

            return Ok(patients);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PatientResponse>> GetById(Guid id)
        {
            var patient = await _dbContext.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(patient => patient.Id == id);

            if (patient is null)
            {
                return PatientNotFound();
            }

            return Ok(ToResponse(patient));
        }

        [HttpPost]
        public async Task<ActionResult<PatientResponse>> Create(PatientRequest request)
        {
            if (!IsBirthDateValid(request))
            {
                return ValidationProblem(ModelState);
            }

            var patient = new Patient();
            ApplyRequest(patient, request);

            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync();

            var response = ToResponse(patient);

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<PatientResponse>> Update(Guid id, PatientRequest request)
        {
            if (!IsBirthDateValid(request))
            {
                return ValidationProblem(ModelState);
            }

            var patient = await _dbContext.Patients.FindAsync(id);

            if (patient is null)
            {
                return PatientNotFound();
            }

            ApplyRequest(patient, request);
            patient.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(ToResponse(patient));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var patient = await _dbContext.Patients.FindAsync(id);

            if (patient is null)
            {
                return PatientNotFound();
            }

            _dbContext.Patients.Remove(patient);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool IsBirthDateValid(PatientRequest request)
        {
            if (request.BirthDate is null)
            {
                return true;
            }

            if (request.BirthDate.Value.Date <= DateTime.UtcNow.Date)
            {
                return true;
            }

            ModelState.AddModelError(nameof(request.BirthDate), "La fecha de nacimiento no puede estar en el futuro.");
            return false;
        }

        private NotFoundObjectResult PatientNotFound()
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Paciente no encontrado.",
                Detail = "No existe un paciente con el identificador indicado."
            });
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
