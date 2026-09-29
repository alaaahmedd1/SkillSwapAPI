using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;

namespace SkillSwapAPI.Domain.Modules.LiveSessions.Entities
{
    public sealed class LiveSessionRoom
    {
        public Guid Id { get; set; }
        public Guid SwapRequestId { get; set; }
        public string RoomToken { get; set; } = null!;
        public DateTimeOffset ScheduledStartTime { get; set; }
        public DateTimeOffset? ActualStartTime { get; set; }
        public DateTimeOffset? ActualEndTime { get; set; }
        public int DurationSeconds { get; set; }
        public LiveSessionStatus Status { get; set; }
    }
}
