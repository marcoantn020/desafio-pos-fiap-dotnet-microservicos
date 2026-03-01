namespace NotificationsAPI.Infrastructure.Persistence;

public class InboxMessage
{
    public Guid Id { get; set; }              // messageId (ou eventId)
    public DateTime ConsumedAtUtc { get; set; }
    public string Consumer { get; set; } = string.Empty;
}