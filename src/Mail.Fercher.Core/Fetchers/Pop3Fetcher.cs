using MailKit.Net.Pop3;
using MailKit.Security;

namespace Mail.Fercher.Core;

public class Pop3Fetcher : IFetcher
{
    public async Task<List<MailMessage>> FetchAsync(MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        using var client = new Pop3Client();

        await ConnectTo(client, mailServerConnection, cancellationToken);

        return await FetchAsync(client, cancellationToken);
    }

    private async Task<List<MailMessage>> FetchAsync(Pop3Client client, CancellationToken cancellationToken)
    {
        var mailMessages = new List<MailMessage>();

        for (int i = 0; i < client.Count; i++)
        {
            var message = await GetMessageAsync(client, i, cancellationToken);
            mailMessages.Add(message);
        }

        await client.DisconnectAsync(true, cancellationToken);

        return mailMessages;
    }

    public async Task<List<MailMessage>> FetchParallelAsync(FetcherConfiguration fetcherConfiguration, MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        using var client = new Pop3Client();

        await ConnectTo(client, mailServerConnection, cancellationToken);

        if (client.Count < fetcherConfiguration.MinimumMessageToEnableParallel)
        {
            return await FetchAsync(client, cancellationToken);
        }

        var indexes = Enumerable.Range(0, client.Count - 1).ToList().AsReadOnly();

        var mailMessages = new List<MailMessage>();

        await Parallel.ForEachAsync(indexes, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = fetcherConfiguration.MaxDegreeOfParallelism
        },
            async (index, taskCancellationToken) =>
            {
                var message = await GetMessageAsync(client, index, taskCancellationToken);
                mailMessages.Add(message);
            });

        await client.DisconnectAsync(true, cancellationToken);

        return mailMessages;
    }

    private async Task ConnectTo(Pop3Client client, MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        await client.ConnectAsync(mailServerConnection.Host, mailServerConnection.Port, SecureSocketOptions.Auto);

        await client.AuthenticateAsync(mailServerConnection.UserName, mailServerConnection.Password);
    }

    private async Task<MailMessage> GetMessageAsync(Pop3Client client, int index, CancellationToken cancellationToken)
    {
        var message = await client.GetMessageAsync(index, cancellationToken);

        await client.DeleteMessageAsync(index, cancellationToken);

        return new() { MimeMessage = message };
    }
}
