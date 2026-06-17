using ProcureFlow.Domain;

namespace ProcureFlow.Tests;

public class ProcurementRequestTests
{
    [Fact]
    public void Submit_requires_at_least_one_item_because_empty_requests_should_not_enter_approval()
    {
        var request = CreateRequest();

        var error = Assert.Throws<DomainException>(() => request.Submit(Guid.NewGuid()));

        Assert.Equal("A request must contain at least one line item before submission.", error.Message);
        Assert.Equal(RequestStatus.Draft, request.Status);
    }

    [Fact]
    public void Approve_requires_submitted_status_because_managers_should_not_approve_drafts()
    {
        var request = CreateRequest();
        request.AddItem("Laptop", 1, 1200m, "Hardware");

        var error = Assert.Throws<DomainException>(() => request.Approve(Guid.NewGuid()));

        Assert.Equal("Only submitted requests can be approved.", error.Message);
        Assert.Equal(RequestStatus.Draft, request.Status);
    }

    [Fact]
    public void Full_workflow_records_logs_because_the_demo_must_show_traceability()
    {
        var actorId = Guid.NewGuid();
        var request = CreateRequest();

        request.AddItem("Laptop", 1, 1200m, "Hardware");
        request.Submit(actorId, "Ready for review");
        request.Approve(actorId, "Approved for onboarding");
        request.MarkOrdered(actorId);
        request.Complete(actorId);

        Assert.Equal(RequestStatus.Completed, request.Status);
        Assert.Equal(1200m, request.EstimatedTotal);
        Assert.Equal(4, request.ApprovalLogs.Count);
        Assert.Equal(RequestStatus.Submitted, request.ApprovalLogs.First().ToStatus);
    }

    [Fact]
    public void Reject_requires_remarks_because_finance_needs_a_reason_in_the_audit_trail()
    {
        var request = CreateRequest();
        request.AddItem("Laptop", 1, 1200m, "Hardware");
        request.Submit(Guid.NewGuid());

        var error = Assert.Throws<DomainException>(() => request.Reject(Guid.NewGuid(), ""));

        Assert.Equal("Rejected requests require remarks.", error.Message);
        Assert.Equal(RequestStatus.Submitted, request.Status);
    }

    private static ProcurementRequest CreateRequest()
    {
        return new ProcurementRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
    }
}
