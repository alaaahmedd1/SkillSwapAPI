using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequests;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using SkillSwapAPI.Domain.Skills.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.SwapRequests;

public class GetSwapRequestsQueryHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    private readonly Guid _requesterId = TestData.UserId;
    private readonly Guid _receiverId = TestData.OtherUserId;
    private readonly Guid _offeredSkillId = Guid.NewGuid();
    private readonly Guid _requestedSkillId = Guid.NewGuid();

    private GetSwapRequestsQueryHandler CreateHandler() => new(_fixture.UnitOfWork, _identityService);

    private SwapRequest CreateSwapRequestWithSkills(
        Guid id, SwapRequestStatus status, bool requesterConfirmed = false, bool receiverConfirmed = false)
    {
        var offeredSkill = TestData.Skill(_offeredSkillId, "C#", 1, TestData.Category(1, "Programming"));
        var requestedSkill = TestData.Skill(_requestedSkillId, "Guitar", 2, TestData.Category(2, "Music"));
        var swap = TestData.SwapRequest(
            id, _requesterId, _receiverId, _offeredSkillId, _requestedSkillId,
            status, requesterConfirmed, receiverConfirmed);
        swap.OfferedSkill = offeredSkill;
        swap.RequestedSkill = requestedSkill;
        return swap;
    }

    private void SetupPage(IReadOnlyList<SwapRequest> items, int totalCount)
    {
        _fixture.SwapRequests
            .GetPagedForUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<SwapRequestStatus?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((items, totalCount)));
    }

    private void SetupProfiles(IReadOnlyList<ProfileIdentityDto> profiles)
    {
        _identityService
            .GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(profiles));
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedPagedResult_WhenUserHasSwapRequests()
    {
        var firstSwapId = Guid.NewGuid();
        var secondSwapId = Guid.NewGuid();
        var firstSwap = CreateSwapRequestWithSkills(firstSwapId, SwapRequestStatus.Pending);
        var secondSwap = CreateSwapRequestWithSkills(
            secondSwapId, SwapRequestStatus.Accepted, receiverConfirmed: true);

        IReadOnlyList<SwapRequest> items = new List<SwapRequest> { firstSwap, secondSwap };
        SetupPage(items, 7);

        IReadOnlyList<ProfileIdentityDto> profiles = new List<ProfileIdentityDto>
        {
            TestData.Profile(_requesterId, "Ada", "Lovelace"),
            TestData.Profile(_receiverId, "Grace", "Hopper")
        };
        SetupProfiles(profiles);

        var query = new GetSwapRequestsQuery(_requesterId, SwapRequestStatus.Pending, 2, 5);

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(7, result.Value.TotalCount);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(5, result.Value.PageSize);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Count);

        var first = result.Value.Items[0];
        Assert.Equal(firstSwapId, first.Id);
        Assert.Equal(_requesterId, first.RequesterId);
        Assert.Equal(_receiverId, first.ReceiverId);
        Assert.Equal("Ada", first.RequesterFirstName);
        Assert.Equal("Lovelace", first.RequesterLastName);
        Assert.Equal("Grace", first.ReceiverFirstName);
        Assert.Equal("Hopper", first.ReceiverLastName);
        Assert.Equal(_offeredSkillId, first.OfferedSkill.SkillId);
        Assert.Equal("C#", first.OfferedSkill.SkillName);
        Assert.Equal("Programming", first.OfferedSkill.CategoryName);
        Assert.Equal(_requestedSkillId, first.RequestedSkill.SkillId);
        Assert.Equal("Guitar", first.RequestedSkill.SkillName);
        Assert.Equal("Music", first.RequestedSkill.CategoryName);
        Assert.Equal(SwapRequestStatus.Pending, first.Status);
        Assert.False(first.IsRequesterConfirmed);
        Assert.False(first.IsReceiverConfirmed);
        Assert.Equal(firstSwap.CreatedAtUtc, first.CreatedAtUtc);

        var second = result.Value.Items[1];
        Assert.Equal(secondSwapId, second.Id);
        Assert.Equal(SwapRequestStatus.Accepted, second.Status);
        Assert.True(second.IsReceiverConfirmed);

        await _fixture.SwapRequests.Received(1).GetPagedForUserAsync(
            _requesterId, SwapRequestStatus.Pending, 2, 5, Arg.Any<CancellationToken>());

        await _identityService.Received(1).GetProfilesAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(_requesterId) && ids.Contains(_receiverId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenUserHasNoSwapRequests()
    {
        IReadOnlyList<SwapRequest> items = new List<SwapRequest>();
        SetupPage(items, 0);
        SetupProfiles(new List<ProfileIdentityDto>());

        var query = new GetSwapRequestsQuery(_requesterId);

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);

        await _fixture.SwapRequests.Received(1).GetPagedForUserAsync(
            _requesterId, null, 1, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapEmptyNames_WhenProfilesAreNotReturned()
    {
        var swap = CreateSwapRequestWithSkills(Guid.NewGuid(), SwapRequestStatus.Pending);

        IReadOnlyList<SwapRequest> items = new List<SwapRequest> { swap };
        SetupPage(items, 1);
        SetupProfiles(new List<ProfileIdentityDto>());

        var query = new GetSwapRequestsQuery(_requesterId, SwapRequestStatus.Pending, 1, 10);

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(string.Empty, item.RequesterFirstName);
        Assert.Equal(string.Empty, item.RequesterLastName);
        Assert.Equal(string.Empty, item.ReceiverFirstName);
        Assert.Equal(string.Empty, item.ReceiverLastName);
        Assert.Equal("C#", item.OfferedSkill.SkillName);
        Assert.Equal("Guitar", item.RequestedSkill.SkillName);
    }
}
