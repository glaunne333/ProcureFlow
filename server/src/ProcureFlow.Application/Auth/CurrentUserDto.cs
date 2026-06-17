using ProcureFlow.Domain;

namespace ProcureFlow.Application.Auth;

public sealed record CurrentUserDto(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    Guid DepartmentId);
