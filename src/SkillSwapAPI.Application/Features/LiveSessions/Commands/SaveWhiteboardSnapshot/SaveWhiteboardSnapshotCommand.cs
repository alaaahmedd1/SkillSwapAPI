using MediatR;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.SaveWhiteboardSnapshot;

public sealed record SaveWhiteboardSnapshotCommand(
    Guid SwapRequestId,
    Guid UserId,
    string CanvasDataJson) : IRequest<Result<WhiteboardSnapshotDto>>;
