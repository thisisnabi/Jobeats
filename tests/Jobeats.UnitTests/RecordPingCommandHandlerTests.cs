using Jobeats.Application.Pings.Commands;
using Jobeats.Core.Entities;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;
using Moq;

namespace Jobeats.UnitTests;

public class RecordPingCommandHandlerTests
{
    private readonly Mock<ICheckRepository> _checkRepositoryMock;
    private readonly Mock<IPingRepository> _pingRepositoryMock;
    private readonly Mock<IFlipRepository> _flipRepositoryMock;
    private readonly RecordPingCommandHandler _handler;

    public RecordPingCommandHandlerTests()
    {
        _checkRepositoryMock = new Mock<ICheckRepository>();
        _pingRepositoryMock = new Mock<IPingRepository>();
        _flipRepositoryMock = new Mock<IFlipRepository>();
        
        _handler = new RecordPingCommandHandler(
            _checkRepositoryMock.Object,
            _pingRepositoryMock.Object,
            _flipRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenCheckNotFound_ReturnsFailure()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Check?)null);
        
        var command = new RecordPingCommand(checkId, PingType.Success);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Success);
        Assert.Equal("Check not found", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WhenCheckIsPaused_ReturnsFailure()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        var check = new Check { Id = checkId, Status = CheckStatus.Paused };
        
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(check);
        
        var command = new RecordPingCommand(checkId, PingType.Success);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Success);
        Assert.Equal("Check is paused", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_SuccessPing_UpdatesCheckAndCreatesPing()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        var check = new Check { Id = checkId, Status = CheckStatus.New };
        
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(check);
        
        _pingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ping p, CancellationToken _) => p);
        
        var command = new RecordPingCommand(checkId, PingType.Success);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.PingId);
        Assert.Equal(CheckStatus.Up, result.NewStatus);
        
        _pingRepositoryMock.Verify(r => r.AddAsync(It.Is<Ping>(p =>
            p.CheckId == checkId && p.Type == PingType.Success
        ), It.IsAny<CancellationToken>()), Times.Once);
        
        _checkRepositoryMock.Verify(r => r.UpdateAsync(check, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FailPing_SetsStatusToDown()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        var check = new Check { Id = checkId, Status = CheckStatus.Up };
        
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(check);
        
        _pingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ping p, CancellationToken _) => p);
        
        var command = new RecordPingCommand(checkId, PingType.Fail);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(CheckStatus.Down, result.NewStatus);
    }

    [Fact]
    public async Task Handle_StatusChange_CreatesFlip()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        var check = new Check { Id = checkId, Status = CheckStatus.Up };
        
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(check);
        
        _pingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ping p, CancellationToken _) => p);
        
        _flipRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Flip>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Flip f, CancellationToken _) => f);
        
        var command = new RecordPingCommand(checkId, PingType.Fail);
        
        // Act
        await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        _flipRepositoryMock.Verify(r => r.AddAsync(It.Is<Flip>(f =>
            f.CheckId == checkId &&
            f.OldStatus == CheckStatus.Up &&
            f.NewStatus == CheckStatus.Down
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StartPing_CalculatesDurationOnSubsequentSuccess()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddSeconds(-10);
        var check = new Check
        {
            Id = checkId,
            Status = CheckStatus.Started,
            LastStartAt = startTime
        };
        
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(check);
        
        Ping? capturedPing = null;
        _pingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
            .Callback<Ping, CancellationToken>((p, _) => capturedPing = p)
            .ReturnsAsync((Ping p, CancellationToken _) => p);
        
        var command = new RecordPingCommand(checkId, PingType.Success);
        
        // Act
        await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(capturedPing);
        Assert.NotNull(capturedPing.DurationMs);
        Assert.True(capturedPing.DurationMs >= 10000); // At least 10 seconds
    }

    [Fact]
    public async Task Handle_ExitStatusZero_SetsStatusToUp()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        var check = new Check { Id = checkId, Status = CheckStatus.Started };
        
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(check);
        
        _pingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ping p, CancellationToken _) => p);
        
        var command = new RecordPingCommand(checkId, PingType.ExitStatus, ExitStatus: 0);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(CheckStatus.Up, result.NewStatus);
    }

    [Fact]
    public async Task Handle_ExitStatusNonZero_SetsStatusToDown()
    {
        // Arrange
        var checkId = Guid.NewGuid();
        var check = new Check { Id = checkId, Status = CheckStatus.Started };
        
        _checkRepositoryMock
            .Setup(r => r.GetByIdAsync(checkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(check);
        
        _pingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ping p, CancellationToken _) => p);
        
        var command = new RecordPingCommand(checkId, PingType.ExitStatus, ExitStatus: 1);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(CheckStatus.Down, result.NewStatus);
    }
}
