using NorvixHub.Domain.Users;

namespace NorvixHub.Application.Tenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid UserId { get; }
    TenantRole Role { get; }
}

