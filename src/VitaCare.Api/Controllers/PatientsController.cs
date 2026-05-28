using Microsoft.AspNetCore.Mvc;
using VitaCare.Api.Contracts.Patients;
using VitaCare.Api.Services.Patients;

namespace VitaCare.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PatientResponse>>> GetAll(
            CancellationToken cancellationToken = default)
        {
            return Ok(await _patientService.GetAllAsync(cancellationToken));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PatientResponse>> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var patient = await _patientService.GetByIdAsync(id, cancellationToken);

            if (patient is null)
            {
                return PatientNotFound();
            }

            return Ok(patient);
        }

        [HttpPost]
        public async Task<ActionResult<PatientResponse>> Create(
            PatientRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!IsBirthDateValid(request))
            {
                return InvalidRequest();
            }

            var response = await _patientService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<PatientResponse>> Update(
            Guid id,
            PatientRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!IsBirthDateValid(request))
            {
                return InvalidRequest();
            }

            var patient = await _patientService.UpdateAsync(id, request, cancellationToken);

            if (patient is null)
            {
                return PatientNotFound();
            }

            return Ok(patient);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var wasDeleted = await _patientService.DeleteAsync(id, cancellationToken);

            if (!wasDeleted)
            {
                return PatientNotFound();
            }

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

        private BadRequestObjectResult InvalidRequest()
        {
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "La solicitud no es válida.",
                Detail = "Revisa los campos enviados e intenta nuevamente."
            });
        }

    }
}
