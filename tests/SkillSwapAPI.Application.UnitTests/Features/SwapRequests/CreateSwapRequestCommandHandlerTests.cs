using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.CreateSwapRequest;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using SkillSwapAPI.Domain.Skills.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.SwapRequests;

public class CreateSwapRequestCommandHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    private readonly Guid _requesterId = TestData.UserId;
    private readonly Guid _receiverId = TestData.OtherUserId;
    private readonly Guid _offeredSkillId = Guid.NewGuid();
    private readonly Guid _requestedSkillId = Guid.NewGuid();

    private CreateSwapRequestCommandHandler CreateHandler() =>
        new(_fixture.UnitOfWork, _identityService);

    private void SetupSkills()
    {
        IEnumerable<Skill> skills = new List<Skill>
        {
            TestData.Skill(_offeredSkillId, "C#", 1, TestData.Category(1, "Programming")),
            TestData.Skill(_requestedSkillId, "Guitar", 2, TestData.Category(2, "Music"))
        };

        _fixture.Skills
            .GetByIdsWithCategoryAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(skills));
    }

    private void SetupProfiles(IReadOnlyList<ProfileIdentityDto> profiles)
    {
        _identityService
            .GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(profiles));
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingSwapRequestAndReturnMappedDto_WhenCommandIsValid()
    {
        var command = new CreateSwapRequestCommand(
            _requesterId, _receiverId, _offeredSkillId, _requestedSkillId, "Weekends, afternoon");

        IReadOnlyList<ProfileIdentityDto> profiles = new List<ProfileIdentityDto>
        {
            TestData.Profile(_requesterId, "Ada", "Lovelace"),
            TestData.Profile(_receiverId, "Grace", "Hopper")
        };

        SetupSkills();
        SetupProfiles(profiles);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(_requesterId, result.Value.RequesterId);
        Assert.Equal(_receiverId, result.Value.ReceiverId);
        Assert.Equal("Ada", result.Value.RequesterFirstName);
        Assert.Equal("Lovelace", result.Value.RequesterLastName);
        Assert.Equal("Grace", result.Value.ReceiverFirstName);
        Assert.Equal("Hopper", result.Value.ReceiverLastName);
        Assert.Equal(_offeredSkillId, result.Value.OfferedSkill.SkillId);
        Assert.Equal("C#", result.Value.OfferedSkill.SkillName);
        Assert.Equal("Programming", result.Value.OfferedSkill.CategoryName);
        Assert.Equal(_requestedSkillId, result.Value.RequestedSkill.SkillId);
        Assert.Equal("Guitar", result.Value.RequestedSkill.SkillName);
        Assert.Equal("Music", result.Value.RequestedSkill.CategoryName);
        Assert.Equal(SwapRequestStatus.Pending, result.Value.Status);
        Assert.False(result.Value.IsRequesterConfirmed);
        Assert.False(result.Value.IsReceiverConfirmed);
        Assert.NotEqual(default, result.Value.CreatedAtUtc);
        Assert.Null(result.Value.UpdatedAtUtc);

        await _fixture.SwapRequests.Received(1).AddAsync(
            Arg.Is<SwapRequest>(swap =>
                swap.RequesterId == _requesterId &&
                swap.ReceiverId == _receiverId &&
                swap.OfferedSkillId == _offeredSkillId &&
                swap.RequestedSkillId == _requestedSkillId &&
                swap.Status == SwapRequestStatus.Pending &&
                !swap.IsRequesterConfirmed &&
                !swap.IsReceiverConfirmed &&
                swap.ProposedScheduleDetails == "Weekends, afternoon"),
            Arg.Any<CancellationToken>());

        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());

        await _fixture.Skills.Received(1).GetByIdsWithCategoryAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(_offeredSkillId) && ids.Contains(_requestedSkillId)),
            Arg.Any<CancellationToken>());

        await _identityService.Received(1).GetProfilesAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(_requesterId) && ids.Contains(_receiverId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapEmptyNames_WhenProfilesAreMissing()
    {
        var command = new CreateSwapRequestCommand(
            _requesterId, _receiverId, _offeredSkillId, _requestedSkillId, null);

        IReadOnlyList<ProfileIdentityDto> profiles = new List<ProfileIdentityDto>();

        SetupSkills();
        SetupProfiles(profiles);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(string.Empty, result.Value.RequesterFirstName);
        Assert.Equal(string.Empty, result.Value.RequesterLastName);
        Assert.Equal(string.Empty, result.Value.ReceiverFirstName);
        Assert.Equal(string.Empty, result.Value.ReceiverLastName);
        Assert.Equal("C#", result.Value.OfferedSkill.SkillName);
        Assert.Equal("Guitar", result.Value.RequestedSkill.SkillName);
    }
}
