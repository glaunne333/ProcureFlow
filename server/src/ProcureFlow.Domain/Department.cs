namespace ProcureFlow.Domain;

public sealed class Department
{
    private Department()
    {
    }

    public Department(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Department name is required.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
}
