using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;

namespace SkillSwapAPI.Domain.Modules.SessionProposals.Entities;

public sealed class SessionProposal
{
    public Guid Id { get; set; }
    public Guid SwapRequestId { get; set; }
    public Guid ProposerId { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Proposed;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
