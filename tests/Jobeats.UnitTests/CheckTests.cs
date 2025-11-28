using Jobeats.Core.Entities;
using Jobeats.Core.Enums;

namespace Jobeats.UnitTests;

public class CheckTests
{
    [Fact]
    public void NewCheck_HasCorrectDefaults()
    {
        // Arrange & Act
        var check = new Check();
        
        // Assert
        Assert.NotEqual(Guid.Empty, Guid.Parse(check.Slug));
        Assert.Equal(CheckStatus.New, check.Status);
        Assert.Equal(86400, check.PeriodSeconds); // Default: 24 hours
        Assert.Equal(3600, check.GraceSeconds); // Default: 1 hour
        Assert.Equal(0, check.TotalPings);
    }
    
    [Fact]
    public void CalculateNextPing_WhenNoPreviousPing_SetsNextPingFromNow()
    {
        // Arrange
        var check = new Check
        {
            PeriodSeconds = 3600, // 1 hour
            GraceSeconds = 300 // 5 minutes
        };
        var beforeCalculation = DateTime.UtcNow;
        
        // Act
        check.CalculateNextPing();
        
        // Assert
        Assert.NotNull(check.NextPingAt);
        Assert.NotNull(check.AlertAfterAt);
        Assert.True(check.NextPingAt >= beforeCalculation.AddSeconds(3600));
        Assert.True(check.AlertAfterAt >= check.NextPingAt.Value.AddSeconds(300));
    }
    
    [Fact]
    public void CalculateNextPing_WhenHasPreviousPing_SetsNextPingFromLastPing()
    {
        // Arrange
        var lastPing = DateTime.UtcNow.AddMinutes(-30);
        var check = new Check
        {
            PeriodSeconds = 3600, // 1 hour
            GraceSeconds = 300, // 5 minutes
            LastPingAt = lastPing
        };
        
        // Act
        check.CalculateNextPing();
        
        // Assert
        Assert.Equal(lastPing.AddSeconds(3600), check.NextPingAt);
        Assert.Equal(lastPing.AddSeconds(3600 + 300), check.AlertAfterAt);
    }
    
    [Fact]
    public void RecordPing_Success_UpdatesStatusAndTimestamps()
    {
        // Arrange
        var check = new Check { Status = CheckStatus.New };
        var pingTime = DateTime.UtcNow;
        
        // Act
        check.RecordPing(PingType.Success, pingTime);
        
        // Assert
        Assert.Equal(CheckStatus.Up, check.Status);
        Assert.Equal(pingTime, check.LastPingAt);
        Assert.Equal(1, check.TotalPings);
        Assert.Null(check.LastStartAt);
    }
    
    [Fact]
    public void RecordPing_Fail_SetsStatusToDown()
    {
        // Arrange
        var check = new Check { Status = CheckStatus.Up };
        var pingTime = DateTime.UtcNow;
        
        // Act
        check.RecordPing(PingType.Fail, pingTime);
        
        // Assert
        Assert.Equal(CheckStatus.Down, check.Status);
        Assert.Equal(pingTime, check.LastPingAt);
    }
    
    [Fact]
    public void RecordPing_Start_SetsStatusToStarted()
    {
        // Arrange
        var check = new Check { Status = CheckStatus.Up };
        var pingTime = DateTime.UtcNow;
        
        // Act
        check.RecordPing(PingType.Start, pingTime);
        
        // Assert
        Assert.Equal(CheckStatus.Started, check.Status);
        Assert.Equal(pingTime, check.LastStartAt);
    }
    
    [Fact]
    public void RecordPing_Log_DoesNotChangeStatus()
    {
        // Arrange
        var originalStatus = CheckStatus.Up;
        var check = new Check { Status = originalStatus };
        var pingTime = DateTime.UtcNow;
        
        // Act
        check.RecordPing(PingType.Log, pingTime);
        
        // Assert
        Assert.Equal(originalStatus, check.Status);
        Assert.Equal(1, check.TotalPings);
    }
    
    [Fact]
    public void RecordPing_Success_ClearsStartTimestamps()
    {
        // Arrange
        var check = new Check
        {
            Status = CheckStatus.Started,
            LastStartAt = DateTime.UtcNow.AddMinutes(-5),
            LastRunId = "run-123"
        };
        var pingTime = DateTime.UtcNow;
        
        // Act
        check.RecordPing(PingType.Success, pingTime);
        
        // Assert
        Assert.Null(check.LastStartAt);
        Assert.Null(check.LastRunId);
    }
}
