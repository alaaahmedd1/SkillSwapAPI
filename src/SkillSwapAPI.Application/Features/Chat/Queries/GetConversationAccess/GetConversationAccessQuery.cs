using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Chat.Queries.GetConversationAccess;

public sealed record GetConversationAccessQuery(
    Guid ConversationId,
    Guid UserId) : IRequest<Result<Success>>;
