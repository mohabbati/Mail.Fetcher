using MailKit;
using MailKit.Security;
using MailKit.Net.Imap;

namespace Mail.Fercher.Core;

public class ImapFetcher : IFetcher
{
    private const int maxDegreeOfParallelism = 2;

    public async Task<List<MailMessage>> FetchAsync(MailServerConnection mailServerConnection, FetchRequest fetchRequest, CancellationToken cancellationToken)
    {
        using var client = new ImapClient();

        await client.ConnectAsync(mailServerConnection.Host, mailServerConnection.Port, SecureSocketOptions.Auto, cancellationToken);

        await client.AuthenticateAsync(mailServerConnection.UserName, mailServerConnection.Password, cancellationToken);

        await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        var uniqueIds = await client.Inbox.SearchAsync(fetchRequest.Query, cancellationToken);

        var mailMessages = new List<MailMessage>();

        await Parallel.ForEachAsync(uniqueIds, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        },
            async (uniqueId, taskCancellationToken) =>
            {
                var message = await client.Inbox.GetMessageAsync(uniqueId, taskCancellationToken);
                mailMessages.Add(new() { MimeMessage = message });
            });

        await client.DisconnectAsync(true, cancellationToken);

        return mailMessages;
    }
}
