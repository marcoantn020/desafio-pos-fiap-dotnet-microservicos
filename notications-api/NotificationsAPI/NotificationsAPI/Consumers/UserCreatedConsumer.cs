using Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationsAPI.Infrastructure.Persistence;

namespace NotificationsAPI.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEventV1>
{
    private readonly NotificationsDbContext _db;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(NotificationsDbContext db, ILogger<UserCreatedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEventV1> context)
    {
        var evt = context.Message;

        var consumerName = nameof(UserCreatedConsumer);
        var already = await _db.InboxMessages.AnyAsync(x => x.Id == evt.EventId && x.Consumer == consumerName);
        if (already)
        {
            _logger.LogInformation("Duplicate ignored: UserCreatedEventV1 {EventId}", evt.EventId);
            return;
        }

        _db.InboxMessages.Add(new InboxMessage
        {
            Id = evt.EventId,
            Consumer = consumerName,
            ConsumedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // Simular envio de e-mail via log
        _logger.LogInformation(
            "[EMAIL] Bem-vindo(a) {DisplayName}! Enviado para {Email}. (UserId={UserId}, EventId={EventId})",
            evt.DisplayName, evt.Email, evt.UserId, evt.EventId
        );
    }
}