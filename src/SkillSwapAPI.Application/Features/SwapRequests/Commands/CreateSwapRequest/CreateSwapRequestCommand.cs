using MediatR;
using SkillSwapAPI.Application.Features.SwapRequests.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CreateSwapRequest;

public sealed record CreateSwapRequestCommand(
    Guid RequesterId,
    Guid ReceiverId,
    Guid OfferedSkillId,
    Guid RequestedSkillId,
    string? ProposedScheduleDetails) : IRequest<Result<SwapRequestDto>>;
