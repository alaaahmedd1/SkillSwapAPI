using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Chat.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Chat.Queries.GetChatHistory;

public sealed record GetChatHistoryQuery(
    Guid ConversationId,
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<MessageDto>>>;
