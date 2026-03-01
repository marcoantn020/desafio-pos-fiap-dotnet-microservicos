namespace Contracts.IntegrationEvents;

public record OrderPlacedEventV1(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    int PriceCents,
    string Currency,
    int SchemaVersion = 1
);
