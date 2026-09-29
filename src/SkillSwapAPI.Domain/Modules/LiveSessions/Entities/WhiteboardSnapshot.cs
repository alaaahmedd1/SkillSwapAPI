using System;

namespace SkillSwapAPI.Domain.Modules.LiveSessions.Entities
{
    public sealed class WhiteboardSnapshot
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public string CanvasDataJson { get; set; } = null!;

        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}
