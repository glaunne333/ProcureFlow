namespace ProcureFlow.Domain;

public sealed class ProcurementRequest
{
    private readonly List<ApprovalLog> _approvalLogs = [];
    private readonly List<RequestItem> _items = [];

    private ProcurementRequest()
    {
    }

    public ProcurementRequest(Guid requestedById, Guid departmentId, Guid vendorId)
    {
        Id = Guid.NewGuid();
        RequestNo = $"PR-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        RequestedById = requestedById;
        DepartmentId = departmentId;
        VendorId = vendorId;
        Status = RequestStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string RequestNo { get; private set; } = string.Empty;
    public Guid RequestedById { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid VendorId { get; private set; }
    public RequestStatus Status { get; private set; }
    public decimal EstimatedTotal { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public IReadOnlyCollection<RequestItem> Items => _items;
    public IReadOnlyCollection<ApprovalLog> ApprovalLogs => _approvalLogs;

    public void AddItem(string description, int quantity, decimal unitCost, string category)
    {
        EnsureStatus(RequestStatus.Draft, "Only draft requests can be edited.");

        var item = new RequestItem(Id, description, quantity, unitCost, category);
        _items.Add(item);
        EstimatedTotal = _items.Sum(x => x.LineTotal);
    }

    public void Submit(Guid actorId, string? remarks = null)
    {
        EnsureStatus(RequestStatus.Draft, "Only draft requests can be submitted.");

        if (_items.Count == 0)
        {
            throw new DomainException("A request must contain at least one line item before submission.");
        }

        SubmittedAt = DateTimeOffset.UtcNow;
        MoveTo(RequestStatus.Submitted, actorId, remarks);
    }

    public void Approve(Guid actorId, string? remarks = null)
    {
        EnsureStatus(RequestStatus.Submitted, "Only submitted requests can be approved.");
        MoveTo(RequestStatus.Approved, actorId, remarks);
    }

    public void Reject(Guid actorId, string remarks)
    {
        EnsureStatus(RequestStatus.Submitted, "Only submitted requests can be rejected.");

        if (string.IsNullOrWhiteSpace(remarks))
        {
            throw new DomainException("Rejected requests require remarks.");
        }

        MoveTo(RequestStatus.Rejected, actorId, remarks);
    }

    public void MarkOrdered(Guid actorId, string? remarks = null)
    {
        EnsureStatus(RequestStatus.Approved, "Only approved requests can be marked ordered.");
        MoveTo(RequestStatus.Ordered, actorId, remarks);
    }

    public void Complete(Guid actorId, string? remarks = null)
    {
        EnsureStatus(RequestStatus.Ordered, "Only ordered requests can be completed.");
        MoveTo(RequestStatus.Completed, actorId, remarks);
    }

    public void Cancel(Guid actorId, string? remarks = null)
    {
        if (Status is RequestStatus.Completed or RequestStatus.Rejected or RequestStatus.Cancelled)
        {
            throw new DomainException("Completed, rejected, or cancelled requests cannot be cancelled.");
        }

        MoveTo(RequestStatus.Cancelled, actorId, remarks);
    }

    private void EnsureStatus(RequestStatus expectedStatus, string message)
    {
        if (Status != expectedStatus)
        {
            throw new DomainException(message);
        }
    }

    private void MoveTo(RequestStatus nextStatus, Guid actorId, string? remarks)
    {
        var previousStatus = Status;
        Status = nextStatus;
        _approvalLogs.Add(new ApprovalLog(Id, actorId, previousStatus, nextStatus, remarks));
    }
}
