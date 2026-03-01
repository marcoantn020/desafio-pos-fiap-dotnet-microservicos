using Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationsAPI.Infrastructure.Persistence;

namespace NotificationsAPI.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEventV1>
{
    private readonly NotificationsDbContext _db;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(NotificationsDbContext db, ILogger<PaymentProcessedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEventV1> context)
    {
        var evt = context.Message;
        
        var consumerName = nameof(PaymentProcessedConsumer);
        var already = await _db.InboxMessages.AnyAsync(x => x.Id == evt.EventId && x.Consumer == consumerName);
        if (already)
        {
            _logger.LogInformation("Duplicate ignored: PaymentProcessedEventV1 {EventId}", evt.EventId);
            return;
        }

        _db.InboxMessages.Add(new InboxMessage
        {
            Id = evt.EventId,
            Consumer = consumerName,
            ConsumedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        if (string.Equals(evt.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "[EMAIL] Compra aprovada | UserId={UserId} OrderId={OrderId} PaymentId={PaymentId} EventId={EventId}",
                evt.UserId, evt.OrderId, evt.PaymentId, evt.EventId
            );
            return;
        }
        
        _logger.LogInformation(
            "[EMAIL] Compra rejeitada ❌ | UserId={UserId} OrderId={OrderId} Reason={Reason} EventId={EventId}",
            evt.UserId, evt.OrderId, evt.Reason ?? "N/A", evt.EventId
        );
    }
}