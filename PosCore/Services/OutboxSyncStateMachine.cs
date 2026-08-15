using System;
using PosDomain.Entities;

namespace PosCore.Services;

public static class OutboxSyncStateMachine
{
    public const int MaxAttempts = 7;
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Processed = "Processed";
    public const string Failed = "Failed";
    public const string DeadLetter = "DeadLetter";

    public static bool IsEligibleForProcessing(OutboxMessage message, DateTime now)
    {
        return message.ProcessedAt == null &&
               !string.Equals(message.Status, DeadLetter, StringComparison.OrdinalIgnoreCase) &&
               message.NextAttemptAt <= now;
    }

    public static void MarkProcessing(OutboxMessage message)
    {
        message.Status = Processing;
    }

    public static void MarkProcessed(OutboxMessage message, DateTime now)
    {
        message.Status = Processed;
        message.ProcessedAt = now;
        message.LastError = string.Empty;
    }

    public static void MarkRetryableFailure(OutboxMessage message, string error, DateTime now)
    {
        message.AttemptCount++;
        message.ProcessedAt = null;
        message.LastError = SanitizeError(error);

        if (message.AttemptCount >= MaxAttempts)
        {
            MarkDeadLetter(message, $"Retry limit reached after {message.AttemptCount} attempts. Last error: {message.LastError}");
            return;
        }

        message.Status = Failed;
        message.NextAttemptAt = now.Add(CalculateBackoff(message.AttemptCount));
    }

    public static void MarkInvalidEvent(OutboxMessage message, DateTime now)
    {
        message.AttemptCount++;
        MarkDeadLetter(message, $"Unsupported outbox event type: {message.EventType}");
        message.NextAttemptAt = now;
    }

    public static void MarkDeadLetter(OutboxMessage message, string reason)
    {
        message.Status = DeadLetter;
        message.ProcessedAt = null;
        message.LastError = SanitizeError(reason);
    }

    public static TimeSpan CalculateBackoff(int attemptCount)
    {
        var cappedAttempt = Math.Clamp(attemptCount, 1, 6);
        return TimeSpan.FromSeconds(Math.Pow(2, cappedAttempt));
    }

    public static string BuildDeterministicClientSideId(OutboxMessage message)
    {
        return $"outbox-{message.EventId}";
    }

    private static string SanitizeError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return "Unknown sync error.";
        }

        var trimmed = error.Replace(Environment.NewLine, " ").Trim();
        return trimmed.Length <= 500 ? trimmed : trimmed[..500];
    }
}
