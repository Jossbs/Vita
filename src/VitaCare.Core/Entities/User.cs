namespace VitaCare.Core.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // We store the Hash, never the plain-text password for security.
        public string PasswordHash { get; set; } = string.Empty;

        // This collection allows us to track every record this specific user has created.
        public ICollection<HealthRecord> CreatedRecords { get; set; } = new List<HealthRecord>();
    }
}