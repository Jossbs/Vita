using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaCare.Api.Contracts.Patients;
using VitaCare.Api.Controllers;
using VitaCare.Core.Entities;
using VitaCare.Core.Enums;
using VitaCare.Infrastructure.Persistence;

namespace VitaCare.Api.Tests;

public class PatientsControllerTests
{
    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedPatientAndPersistsNormalizedValues()
    {
        await using var dbContext = CreateDbContext();
        var controller = new PatientsController(dbContext);
        var request = CreateValidRequest();
        request.FullName = "  María López  ";
        request.EmergencyContactName = "  Ana López  ";

        var result = await controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(PatientsController.GetById), created.ActionName);

        var response = Assert.IsType<PatientResponse>(created.Value);
        Assert.Equal("María López", response.FullName);
        Assert.Equal("Ana López", response.EmergencyContactName);
        Assert.Equal(CareLevel.Moderate, response.CareLevel);
        Assert.Equal(MobilityStatus.NeedsAssistance, response.MobilityStatus);

        var patient = await dbContext.Patients.SingleAsync();
        Assert.Equal(response.Id, patient.Id);
        Assert.Equal("María López", patient.FullName);
        Assert.Equal("Ana López", patient.EmergencyContactName);
    }

    [Fact]
    public async Task Create_WithFutureBirthDate_ReturnsSpanishValidationProblemAndDoesNotPersist()
    {
        await using var dbContext = CreateDbContext();
        var controller = new PatientsController(dbContext);
        var request = CreateValidRequest();
        request.BirthDate = DateTime.UtcNow.Date.AddDays(1);

        var result = await controller.Create(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal("La solicitud no es válida.", problem.Title);

        var error = Assert.Single(problem.Errors[nameof(PatientRequest.BirthDate)]);
        Assert.Equal("La fecha de nacimiento no puede estar en el futuro.", error);
        Assert.Empty(await dbContext.Patients.ToListAsync());
    }

    [Fact]
    public async Task GetById_WhenPatientDoesNotExist_ReturnsSpanishProblemDetails()
    {
        await using var dbContext = CreateDbContext();
        var controller = new PatientsController(dbContext);

        var result = await controller.GetById(Guid.NewGuid());

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(notFound.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Paciente no encontrado.", problem.Title);
        Assert.Equal("No existe un paciente con el identificador indicado.", problem.Detail);
    }

    [Fact]
    public async Task Update_WithExistingPatient_ReturnsUpdatedPatientAndSetsUpdatedAt()
    {
        await using var dbContext = CreateDbContext();
        var patient = new Patient
        {
            FullName = "Nombre original",
            BirthDate = new DateTime(1950, 1, 1),
            CareLevel = CareLevel.Basic,
            MobilityStatus = MobilityStatus.Independent
        };
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var controller = new PatientsController(dbContext);
        var request = CreateValidRequest();
        request.FullName = "Nombre actualizado";
        request.CareLevel = CareLevel.High;

        var result = await controller.Update(patient.Id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PatientResponse>(ok.Value);
        Assert.Equal("Nombre actualizado", response.FullName);
        Assert.Equal(CareLevel.High, response.CareLevel);
        Assert.NotNull(response.UpdatedAt);

        var persisted = await dbContext.Patients.SingleAsync();
        Assert.Equal("Nombre actualizado", persisted.FullName);
        Assert.NotNull(persisted.UpdatedAt);
    }

    [Fact]
    public async Task Delete_WithExistingPatient_RemovesPatient()
    {
        await using var dbContext = CreateDbContext();
        var patient = new Patient
        {
            FullName = "Paciente a eliminar",
            BirthDate = new DateTime(1942, 5, 10),
            CareLevel = CareLevel.Basic,
            MobilityStatus = MobilityStatus.Independent
        };
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var controller = new PatientsController(dbContext);

        var result = await controller.Delete(patient.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(await dbContext.Patients.ToListAsync());
    }

    [Fact]
    public void PatientRequest_WithoutFullName_ReturnsSpanishValidationMessage()
    {
        var request = CreateValidRequest();
        request.FullName = null;
        var context = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

        Assert.False(isValid);
        var error = Assert.Single(validationResults, result => result.MemberNames.Contains(nameof(PatientRequest.FullName)));
        Assert.Equal("El nombre completo es obligatorio.", error.ErrorMessage);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PatientRequest CreateValidRequest()
    {
        return new PatientRequest
        {
            FullName = "María López",
            PreferredName = "María",
            BirthDate = new DateTime(1948, 3, 12),
            BloodType = "O+",
            CareLevel = CareLevel.Moderate,
            MobilityStatus = MobilityStatus.NeedsAssistance,
            RequiresContinuousSupervision = false,
            RequiresMedicationAssistance = true,
            RequiresFeedingAssistance = false,
            PrimaryCondition = "Diabetes tipo 2",
            Allergies = "Penicilina",
            CurrentMedications = "Metformina",
            CareInstructions = "Controlar glucosa antes del desayuno.",
            EmergencyContactName = "Ana López",
            EmergencyContactPhone = "+52 555 010 1000",
            EmergencyContactRelationship = "Hija",
            MedicalNotes = "Requiere seguimiento semanal."
        };
    }
}
