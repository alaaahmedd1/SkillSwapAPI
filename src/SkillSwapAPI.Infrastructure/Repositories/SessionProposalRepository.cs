using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.SessionProposals.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories;

public class SessionProposalRepository(ApplicationDbContext context)
    : BaseRepository<SessionProposal>(context), ISessionProposalRepository
{
}
