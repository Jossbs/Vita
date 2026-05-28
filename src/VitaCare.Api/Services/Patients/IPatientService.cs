using VitaCare.Api.Contracts.Patients;

namespace VitaCare.Api.Services.Patients
{
    public interface IPatientService
    {
        Task<IReadOnlyList<PatientResponse>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PatientResponse> CreateAsync(PatientRequest request, CancellationToken cancellationToken = default);

        Task<PatientResponse?> UpdateAsync(Guid id, PatientRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
