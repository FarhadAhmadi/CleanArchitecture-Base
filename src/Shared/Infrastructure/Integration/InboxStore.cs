using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Integration;

internal sealed class InboxStore(ApplicationDbContext dbContext) : IInboxStore
{
    public async Task<bool> TryStartAsync(
        string messageId,
        string messageType,
        string? payload,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.Set<InboxMessage>()
            .AnyAsync(x => x.MessageId == messageId, cancellationToken);

        if (exists)
        {
            return false;
        }

        dbContext.Set<InboxMessage>().Add(new InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            Type = messageType,
            Payload = payload,
            ReceivedOnUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task MarkProcessedAsync(string messageId, CancellationToken cancellationToken)
    {
        InboxMessage? item = await dbContext.Set<InboxMessage>()
            .SingleOrDefaultAsync(x => x.MessageId == messageId, cancellationToken);

        if (item is null)
        {
            return;
        }

        item.ProcessedOnUtc = DateTime.UtcNow;
        item.Error = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(string messageId, string error, CancellationToken cancellationToken)
    {
        InboxMessage? item = await dbContext.Set<InboxMessage>()
            .SingleOrDefaultAsync(x => x.MessageId == messageId, cancellationToken);

        if (item is null)
        {
            return;
        }

        item.Error = error;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
