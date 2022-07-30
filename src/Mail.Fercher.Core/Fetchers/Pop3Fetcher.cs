using MailKit.Net.Pop3;
using MailKit.Security;
using System.Net.Mail;

namespace Mail.Fercher.Core;

public class Pop3Fetcher : IFetcher
{
    private static int maxDegreeOfParallelism = 3;

    public async Task<List<MailMessage>> FetchAsync(MailServerConnection mailServerConnection, FetchRequest fetchRequest, CancellationToken cancellationToken)
    {
        using var client = new Pop3Client();

        await client.ConnectAsync(mailServerConnection.Host, mailServerConnection.Port, SecureSocketOptions.Auto);

        await client.AuthenticateAsync(mailServerConnection.UserName, mailServerConnection.Password);

        var mailMessages = new List<MailMessage>();

        var indexes = Enumerable.Range(0, mailMessages.Count - 1).ToList();

        await Parallel.ForEachAsync(indexes, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        },
            async (index, taskCancellationToken) =>
            {
                var message = await client.GetMessageAsync(index, taskCancellationToken);
                mailMessages.Add(new() { MimeMessage = message });
                await client.DeleteMessageAsync(index, cancellationToken);
            });

        await client.DisconnectAsync(true, cancellationToken);

        return mailMessages;
    }
}
