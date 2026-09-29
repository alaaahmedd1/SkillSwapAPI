using FluentAssertions;
using Moq;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.SessionProposals.Commands.AcceptProposal;
using SkillSwapAPI.Application.Features.SessionProposals.Commands.CreateProposal;
using SkillSwapAPI.Application.Features.SessionProposals.Commands.RejectProposal;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.SessionProposals.Entities;
using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests
{
    public class CreateProposalCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly CreateProposalCommandHandler _sut;

        public CreateProposalCommandHandlerTests()
        {
            _sut = new CreateProposalCommandHandler(_uow.Object);
        }

        private static SwapRequest MakeSwap(Guid requesterId, Guid receiverId, SwapRequestStatus status)
            => new()
            {
                Id = Guid.NewGuid(),
                RequesterId = requesterId,
                ReceiverId = receiverId,
                Status = status
            };

        private CreateProposalCommand MakeValidCommand(Guid swapId, Guid proposerId) => new(
            swapId, proposerId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(10, 30),
            30);

        [Fact]
        public async Task Handle_WithValidProposal_ReturnsSuccessAndPersists()
        {
            var requesterId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, receiverId, SwapRequestStatus.Pending);
            var command = MakeValidCommand(swap.Id, requesterId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SessionProposal?)null);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeFalse();
            result.Value.Status.Should().Be(ProposalStatus.Proposed);
            result.Value.SwapRequestId.Should().Be(swap.Id);

            _uow.Verify(u => u.SessionProposals.AddAsync(
                It.Is<SessionProposal>(p => p.ProposerId == requesterId && p.DurationMinutes == 30),
                It.IsAny<CancellationToken>()), Times.Once);

            _uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(45)]
        [InlineData(90)]
        [InlineData(15)]
        public async Task Handle_WithDisallowedDuration_ReturnsInvalidDurationError(int badDuration)
        {
            var command = new CreateProposalCommand(
                Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                new TimeOnly(10, 0),
                new TimeOnly(10, 0).AddMinutes(badDuration),
                badDuration);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.InvalidDuration);

            _uow.Verify(u => u.SwapRequests.FindAsync(
                It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenEndTimeDoesNotMatchStartTimePlusDuration_ReturnsTimeMismatchError()
        {
            var command = new CreateProposalCommand(
                Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                new TimeOnly(10, 0),
                new TimeOnly(11, 0),
                30);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.TimeMismatch);
        }

        [Fact]
        public async Task Handle_WithScheduledDateInThePast_ReturnsDateInPastError()
        {
            var command = new CreateProposalCommand(
                Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                new TimeOnly(10, 0),
                new TimeOnly(10, 30),
                30);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.DateInPast);
        }

        [Fact]
        public async Task Handle_WhenSwapRequestDoesNotExist_ReturnsSwapNotFoundError()
        {
            var command = MakeValidCommand(Guid.NewGuid(), Guid.NewGuid());

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SwapRequest?)null);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.SwapNotFound);
        }

        [Fact]
        public async Task Handle_WhenProposerIsNotAParticipant_ReturnsNotParticipantError()
        {
            var swap = MakeSwap(Guid.NewGuid(), Guid.NewGuid(), SwapRequestStatus.Pending);
            var strangerId = Guid.NewGuid();
            var command = MakeValidCommand(swap.Id, strangerId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.NotParticipant);
        }

        [Theory]
        [InlineData(SwapRequestStatus.Rejected)]
        [InlineData(SwapRequestStatus.Cancelled)]
        [InlineData(SwapRequestStatus.Completed)]
        public async Task Handle_WhenSwapIsInTerminalState_ReturnsInvalidSwapStateError(SwapRequestStatus terminalStatus)
        {
            var requesterId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, Guid.NewGuid(), terminalStatus);
            var command = MakeValidCommand(swap.Id, requesterId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.InvalidSwapState);
        }

        [Fact]
        public async Task Handle_WhenAnActiveProposalAlreadyExists_ReturnsActiveProposalExistsError_AndDoesNotInsert()
        {
            var requesterId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, Guid.NewGuid(), SwapRequestStatus.Pending);
            var command = MakeValidCommand(swap.Id, requesterId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SessionProposal { Id = Guid.NewGuid(), Status = ProposalStatus.Proposed });

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.ActiveProposalExists);

            _uow.Verify(u => u.SessionProposals.AddAsync(
                It.IsAny<SessionProposal>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    public class AcceptProposalCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<ILiveSessionTokenProvider> _tokenProvider = new();
        private readonly AcceptProposalCommandHandler _sut;

        public AcceptProposalCommandHandlerTests()
        {
            _tokenProvider.Setup(t => t.GenerateRoomToken()).Returns("fake-room-token");
            _sut = new AcceptProposalCommandHandler(_uow.Object, _tokenProvider.Object);
        }

        private static SwapRequest MakeSwap(Guid requesterId, Guid receiverId, SwapRequestStatus status) => new()
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = status
        };

        private static SessionProposal MakeProposal(Guid swapId, Guid proposerId, ProposalStatus status) => new()
        {
            Id = Guid.NewGuid(),
            SwapRequestId = swapId,
            ProposerId = proposerId,
            ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30),
            DurationMinutes = 30,
            Status = status
        };

        [Fact]
        public async Task Handle_WhenNonProposerAcceptsAPendingProposal_UpdatesSwapProposalAndRoom()
        {
            var requesterId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, receiverId, SwapRequestStatus.Pending);
            var proposal = MakeProposal(swap.Id, requesterId, ProposalStatus.Proposed);
            var command = new AcceptProposalCommand(swap.Id, proposal.Id, receiverId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            _uow.Setup(u => u.Conversations.FindAsync(
                    It.IsAny<Expression<Func<Conversation, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Conversation?)null);

            _uow.Setup(u => u.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((LiveSessionRoom?)null);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeFalse();

            proposal.Status.Should().Be(ProposalStatus.Accepted);
            swap.Status.Should().Be(SwapRequestStatus.Accepted);

            _uow.Verify(u => u.Conversations.AddAsync(
                It.Is<Conversation>(c => c.SwapRequestId == swap.Id), It.IsAny<CancellationToken>()), Times.Once);

            _uow.Verify(u => u.LiveSessionRooms.AddAsync(
                It.Is<LiveSessionRoom>(r =>
                    r.SwapRequestId == swap.Id &&
                    r.ScheduledStartTime == new DateTimeOffset(
                        proposal.ScheduledDate.ToDateTime(proposal.StartTime, DateTimeKind.Utc))),
                It.IsAny<CancellationToken>()), Times.Once);

            _uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenProposerTriesToAcceptOwnProposal_ReturnsErrorAndChangesNothing()
        {
            var requesterId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, Guid.NewGuid(), SwapRequestStatus.Pending);
            var proposal = MakeProposal(swap.Id, requesterId, ProposalStatus.Proposed);
            var command = new AcceptProposalCommand(swap.Id, proposal.Id, requesterId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.CannotAcceptOwnProposal);

            proposal.Status.Should().Be(ProposalStatus.Proposed);
            swap.Status.Should().Be(SwapRequestStatus.Pending);

            _uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserIsNotAParticipant_ReturnsNotParticipantError()
        {
            var swap = MakeSwap(Guid.NewGuid(), Guid.NewGuid(), SwapRequestStatus.Pending);
            var strangerId = Guid.NewGuid();
            var command = new AcceptProposalCommand(swap.Id, Guid.NewGuid(), strangerId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.NotParticipant);
        }

        [Theory]
        [InlineData(ProposalStatus.Accepted)]
        [InlineData(ProposalStatus.Rejected)]
        public async Task Handle_WhenProposalIsNotInProposedStatus_ReturnsProposalNotPendingError(ProposalStatus status)
        {
            var requesterId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, receiverId, SwapRequestStatus.Pending);
            var proposal = MakeProposal(swap.Id, requesterId, status);
            var command = new AcceptProposalCommand(swap.Id, proposal.Id, receiverId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.ProposalNotPending);
        }

        [Fact]
        public async Task Handle_WhenProposalDoesNotExist_ReturnsProposalNotFoundError()
        {
            var requesterId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, receiverId, SwapRequestStatus.Pending);
            var command = new AcceptProposalCommand(swap.Id, Guid.NewGuid(), receiverId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SessionProposal?)null);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.ProposalNotFound);
        }

        [Fact]
        public async Task Handle_WhenLiveSessionRoomAlreadyExists_UpdatesInsteadOfCreatingDuplicate()
        {
            var requesterId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, receiverId, SwapRequestStatus.Accepted);
            var proposal = MakeProposal(swap.Id, requesterId, ProposalStatus.Proposed);
            var command = new AcceptProposalCommand(swap.Id, proposal.Id, receiverId);

            var existingRoom = new LiveSessionRoom
            {
                Id = Guid.NewGuid(),
                SwapRequestId = swap.Id,
                RoomToken = "old-token",
                ScheduledStartTime = DateTimeOffset.UtcNow.AddDays(5)
            };

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            _uow.Setup(u => u.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRoom);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeFalse();

            existingRoom.ScheduledStartTime.Should().Be(new DateTimeOffset(
                proposal.ScheduledDate.ToDateTime(proposal.StartTime, DateTimeKind.Utc)));

            _uow.Verify(u => u.LiveSessionRooms.AddAsync(
                It.IsAny<LiveSessionRoom>(), It.IsAny<CancellationToken>()), Times.Never);

            _uow.Verify(u => u.LiveSessionRooms.Update(existingRoom), Times.Once);

            _uow.Verify(u => u.Conversations.AddAsync(
                It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    public class RejectProposalCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly RejectProposalCommandHandler _sut;

        public RejectProposalCommandHandlerTests()
        {
            _sut = new RejectProposalCommandHandler(_uow.Object);
        }

        private static SwapRequest MakeSwap(Guid requesterId, Guid receiverId, SwapRequestStatus status) => new()
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = status
        };

        private static SessionProposal MakeProposal(Guid swapId, Guid proposerId, ProposalStatus status) => new()
        {
            Id = Guid.NewGuid(),
            SwapRequestId = swapId,
            ProposerId = proposerId,
            Status = status
        };

        [Fact]
        public async Task Handle_WhenNonProposerRejectsAPendingProposal_UpdatesProposalOnly_SwapUntouched()
        {
            var requesterId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, receiverId, SwapRequestStatus.Pending);
            var proposal = MakeProposal(swap.Id, requesterId, ProposalStatus.Proposed);
            var command = new RejectProposalCommand(swap.Id, proposal.Id, receiverId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeFalse();
            proposal.Status.Should().Be(ProposalStatus.Rejected);
            swap.Status.Should().Be(SwapRequestStatus.Pending);

            _uow.Verify(u => u.SwapRequests.Update(It.IsAny<SwapRequest>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenProposerTriesToRejectOwnProposal_ReturnsCannotRejectOwnProposalError_AndChangesNothing()
        {
            var requesterId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, Guid.NewGuid(), SwapRequestStatus.Pending);
            var proposal = MakeProposal(swap.Id, requesterId, ProposalStatus.Proposed);
            var command = new RejectProposalCommand(swap.Id, proposal.Id, requesterId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            // ✅ اتصلح: بقى بيستخدم CannotRejectOwnProposal الصح، مش الـ Accept error القديم
            result.TopError.Should().Be(ApplicationErrors.Scheduling.CannotRejectOwnProposal);

            proposal.Status.Should().Be(ProposalStatus.Proposed);
            _uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenProposalNotInProposedStatus_ReturnsProposalNotPendingError()
        {
            var requesterId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var swap = MakeSwap(requesterId, receiverId, SwapRequestStatus.Pending);
            var proposal = MakeProposal(swap.Id, requesterId, ProposalStatus.Accepted);
            var command = new RejectProposalCommand(swap.Id, proposal.Id, receiverId);

            _uow.Setup(u => u.SwapRequests.FindAsync(
                    It.IsAny<Expression<Func<SwapRequest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(swap);

            _uow.Setup(u => u.SessionProposals.FindAsync(
                    It.IsAny<Expression<Func<SessionProposal, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationErrors.Scheduling.ProposalNotPending);
        }
    }
}