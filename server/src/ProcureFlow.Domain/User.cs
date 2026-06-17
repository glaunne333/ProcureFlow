namespace ProcureFlow.Domain;

public sealed class User
{
    private User()
    {
    }

    public User(string name, string email, string passwordHash, UserRole role, Guid departmentId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("User name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("User email is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("User password hash is required.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        DepartmentId = departmentId;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid DepartmentId { get; private set; }
    public bool IsActive { get; private set; }
}
