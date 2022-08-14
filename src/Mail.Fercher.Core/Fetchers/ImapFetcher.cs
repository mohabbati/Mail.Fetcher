using MailKit;
using MailKit.Security;
using MailKit.Net.Imap;
using System.Collections.Concurrent;

namespace Mail.Fercher.Core;

public class ImapFetcher : IFetcher
{
    private readonly FetchRequest _fetchRequest;

    public ImapFetcher()
    {
        _fetchRequest = new FetchRequest { Query = MailKit.Search.SearchQuery.All };
    }
    
    public ImapFetcher(FetchRequest fetchRequest)
    {
        _fetchRequest = fetchRequest;
    }

    public async Task<List<MailMessage>> FetchAsync(MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        using var client = new ImapClient();

        await ConnectTo(client, mailServerConnection, cancellationToken);

        return await FetchAsync(client, cancellationToken);
    }

    private async Task<List<MailMessage>> FetchAsync(ImapClient client, CancellationToken cancellationToken)
    {
        await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        var uniqueIds = await client.Inbox.SearchAsync(_fetchRequest.Query, cancellationToken);

        var mailMessages = new LinkedList<MailMessage>();

        foreach (var item in uniqueIds)
        {
            var message = client.Inbox.GetMessage(item);
            mailMessages.AddLast(new MailMessage() { MimeMessage = message });
        }

        await client.DisconnectAsync(true, cancellationToken);

        return mailMessages.ToList();
    }

    public async Task<List<MailMessage>> FetchParallelAsync(FetcherConfiguration fetcherConfiguration, MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        using var client = new ImapClient();

        await ConnectTo(client, mailServerConnection, cancellationToken);

        await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        var uniqueIds = await client.Inbox.SearchAsync(_fetchRequest.Query, cancellationToken);

        if (uniqueIds.Count < fetcherConfiguration.MinimumMessageToEnableParallel)
        {
            return await FetchAsync(client, cancellationToken);
        }

        var mailMessages = new ConcurrentBag<MailMessage>();

        await Parallel.ForEachAsync(uniqueIds, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = fetcherConfiguration.MaxDegreeOfParallelism
        },
            async (uniqueId, taskCancellationToken) =>
            {
                var message = await client.Inbox.GetMessageAsync(uniqueId, taskCancellationToken);
                mailMessages.Add(new() { MimeMessage = message });
            });

        await client.DisconnectAsync(true, cancellationToken);

        return mailMessages.ToList();
    }

    private async Task ConnectTo(ImapClient client, MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        await client.ConnectAsync(mailServerConnection.Host, mailServerConnection.Port, SecureSocketOptions.Auto, cancellationToken);

        await client.AuthenticateAsync(mailServerConnection.UserName, mailServerConnection.Password, cancellationToken);
    }
}
