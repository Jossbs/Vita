using System;

namespace VitaCare.Core.Entities
{
    /// <summary>
    /// Base class for all domain entities to ensure traceability and unique identification.
    /// </summary>
    public abstract class BaseEntity
    {
        // GUID provides higher security and global uniqueness for health records.
        public Guid Id { get; set; } = Guid.NewGuid();

        // Audit field to track when the record was first created.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Track the last time the record was modified.
        public DateTime? UpdatedAt { get; set; }
    }
}
