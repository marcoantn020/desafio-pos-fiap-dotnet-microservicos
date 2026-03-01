namespace Contracts.IntegrationEvents;

public record PaymentProcessedEventV1(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid PaymentId,
    Guid OrderId,
    Guid UserId,
    string Status,        // "Approved" | "Rejected"
    string? Reason,
    int SchemaVersion = 1
);