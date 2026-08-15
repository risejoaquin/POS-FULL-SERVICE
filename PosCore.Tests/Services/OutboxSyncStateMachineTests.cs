using System;
using PosCore.Services;
using PosDomain.Entities;
using Xunit;

namespace PosCore.Tests.Services;

public class OutboxSyncStateMachineTests
{
    [Fact]
    public void MarkRetryableFailure_Should_Set_Failed_State_And_Backoff_Without_ProcessedAt()
    {
        var now = new DateTime(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);
        var message = new OutboxMessage
        {
            EventType = "OrderCreated",
            Status = OutboxSyncStateMachine.Pending,
            NextAttemptAt = now
        };

        OutboxSyncStateMachine.MarkRetryableFailure(message, "network timeout", now);

        Assert.Equal(OutboxSyncStateMachine.Failed, message.Status);
        Assert.Equal(1, message.AttemptCount);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(now.AddSeconds(2), message.NextAttemptAt);
        Assert.Contains("network timeout", message.LastError);
    }

    [Fact]
    public void MarkRetryableFailure_Should_Move_To_DeadLetter_After_MaxAttempts()
    {
        var now = new DateTime(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);
        var message = new OutboxMessage
        {
            EventType = "OrderCreated",
            Status = OutboxSyncStateMachine.Failed,
            AttemptCount = OutboxSyncStateMachine.MaxAttempts - 1,
            ProcessedAt = null
        };

        OutboxSyncStateMachine.MarkRetryableFailure(message, "remote rejected message", now);

        Assert.Equal(OutboxSyncStateMachine.DeadLetter, message.Status);
        Assert.Equal(OutboxSyncStateMachine.MaxAttempts, message.AttemptCount);
        Assert.Null(message.ProcessedAt);
        Assert.Contains("Retry limit reached", message.LastError);
    }

    [Fact]
    public void MarkInvalidEvent_Should_Move_To_DeadLetter_Without_Marking_Processed()
    {
        var now = new DateTime(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);
        var message = new OutboxMessage
        {
            EventType = "UnknownEvent",
            Status = OutboxSyncStateMachine.Pending,
            ProcessedAt = null
        };

        OutboxSyncStateMachine.MarkInvalidEvent(message, now);

        Assert.Equal(OutboxSyncStateMachine.DeadLetter, message.Status);
        Assert.Null(message.ProcessedAt);
        Assert.Contains("UnknownEvent", message.LastError);
    }

    [Fact]
    public void MarkProcessed_Should_Set_ProcessedAt_Only_For_Success()
    {
        var now = new DateTime(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);
        var message = new OutboxMessage
        {
            EventId = "evt-123",
            EventType = "OrderCreated",
            Status = OutboxSyncStateMachine.Processing,
            LastError = "old error"
        };

        OutboxSyncStateMachine.MarkProcessed(message, now);

        Assert.Equal(OutboxSyncStateMachine.Processed, message.Status);
        Assert.Equal(now, message.ProcessedAt);
        Assert.Equal(string.Empty, message.LastError);
        Assert.Equal("evt-123", message.EventId);
    }

    [Fact]
    public void BuildDeterministicClientSideId_Should_Reuse_EventId_For_Idempotent_Retries()
    {
        var message = new OutboxMessage
        {
            EventId = "evt-abc"
        };

        var firstAttempt = OutboxSyncStateMachine.BuildDeterministicClientSideId(message);
        var retryAttempt = OutboxSyncStateMachine.BuildDeterministicClientSideId(message);

        Assert.Equal("outbox-evt-abc", firstAttempt);
        Assert.Equal(firstAttempt, retryAttempt);
    }

    [Fact]
    public void IsEligibleForProcessing_Should_Exclude_DeadLetter_And_Future_Backoff()
    {
        var now = new DateTime(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(OutboxSyncStateMachine.IsEligibleForProcessing(new OutboxMessage
        {
            Status = OutboxSyncStateMachine.Failed,
            ProcessedAt = null,
            NextAttemptAt = now
        }, now));

        Assert.False(OutboxSyncStateMachine.IsEligibleForProcessing(new OutboxMessage
        {
            Status = OutboxSyncStateMachine.DeadLetter,
            ProcessedAt = null,
            NextAttemptAt = now
        }, now));

        Assert.False(OutboxSyncStateMachine.IsEligibleForProcessing(new OutboxMessage
        {
            Status = OutboxSyncStateMachine.Failed,
            ProcessedAt = null,
            NextAttemptAt = now.AddMinutes(1)
        }, now));
    }
}
