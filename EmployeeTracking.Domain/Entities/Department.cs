using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class Department : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Department() { }

        public static Department Create(string name, string? description = null) =>
            new() { Id = Guid.NewGuid(), Name = name, Description = description };
    }

}
