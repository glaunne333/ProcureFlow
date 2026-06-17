namespace ProcureFlow.Domain;

public sealed class RequestItem
{
    private RequestItem()
    {
    }

    public RequestItem(Guid requestId, string description, int quantity, decimal unitCost, string category)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Item description is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Item quantity must be greater than zero.");
        }

        if (unitCost < 0)
        {
            throw new DomainException("Item unit cost cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new DomainException("Item category is required.");
        }

        Id = Guid.NewGuid();
        RequestId = requestId;
        Description = description.Trim();
        Quantity = quantity;
        UnitCost = unitCost;
        Category = category.Trim();
    }

    public Guid Id { get; private set; }
    public Guid RequestId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal LineTotal => Quantity * UnitCost;
}
