namespace ProcureFlow.Domain;

public sealed class ApprovalLog
{
    private ApprovalLog()
    {
    }

    public ApprovalLog(Guid requestId, Guid actorId, RequestStatus fromStatus, RequestStatus toStatus, string? remarks)
    {
        Id = Guid.NewGuid();
        RequestId = requestId;
        ActorId = actorId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid RequestId { get; private set; }
    public Guid ActorId { get; private set; }
    public RequestStatus FromStatus { get; private set; }
    public RequestStatus ToStatus { get; private set; }
    public string? Remarks { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
