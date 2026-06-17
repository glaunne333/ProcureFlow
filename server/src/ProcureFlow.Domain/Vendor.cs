namespace ProcureFlow.Domain;

public sealed class Vendor
{
    private Vendor()
    {
    }

    public Vendor(string name, string? contactEmail = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Vendor name is required.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim().ToLowerInvariant();
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ContactEmail { get; private set; }
    public bool IsActive { get; private set; }
}
