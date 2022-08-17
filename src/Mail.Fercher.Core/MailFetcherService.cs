using Mail.Fercher.Core.Models;
using System.Collections.Concurrent;

namespace Mail.Fercher.Core;

public class MailFetcherService
{
    private readonly IServiceProvider _serviceProvider;
    private List<MailFetcherRequest> _mailFetcherRequests; //TODO: use builder to create new object

    public MailFetcherService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Configure fetcher to fetch messages from the mail server
    /// </summary>
    /// <param name="mailFetcherRequest"></param>
    public void ConfigureFetchers(MailFetcherRequest mailFetcherRequest)
    {
        ArgumentNullException.ThrowIfNull(mailFetcherRequest);

        _mailFetcherRequests.Add(mailFetcherRequest);
    }

    /// <summary>
    /// Configure fetchers to fetch messages from the mail servers
    /// </summary>
    /// <param name="mailFetcherRequest"></param>
    public void ConfigureFetchers(List<MailFetcherRequest> mailFetcherRequest)
    {
        ArgumentNullException.ThrowIfNull(mailFetcherRequest);

        _mailFetcherRequests = mailFetcherRequest;
    }

    /// <summary>
    /// Fetch mail messages from the server based on all the configation.
    /// </summary>
    /// <param name="mailFetcherRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<MailMessage>> InvokeAsync(CancellationToken cancellationToken)
    {
        return await InvokeAsync(new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 1
        }, cancellationToken);
    }

    /// <summary>
    /// Fetch mail messages from the servers list based on all the configation.
    /// </summary>
    /// <param name="parallelOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<MailMessage>> InvokeAsync(ParallelOptions parallelOptions, CancellationToken cancellationToken)
    {
        var allMailMessages = new ConcurrentBag<MailMessage>();

        var imapFetcher = (MailFetcher<IFetcher>?)_serviceProvider.GetService(typeof(MailFetcher<ImapFetcher>));
        var pop3Fetcher = (MailFetcher<IFetcher>?)_serviceProvider.GetService(typeof(MailFetcher<Pop3Fetcher>));

        ArgumentNullException.ThrowIfNull(imapFetcher);
        ArgumentNullException.ThrowIfNull(pop3Fetcher);

        await Parallel.ForEachAsync(_mailFetcherRequests, parallelOptions, async (item, taskCancellationToken) =>
        {
            List<MailMessage> fetcherMailMessages;
            if (item.ProtocolType == ProtocolType.Imap)
            {
                fetcherMailMessages = await GetFetcherMailMessages(item, imapFetcher, cancellationToken);
            }
            else if (item.ProtocolType == ProtocolType.Pop3)
            {
                fetcherMailMessages = await GetFetcherMailMessages(item, pop3Fetcher, cancellationToken);
            }
            else
            {
                fetcherMailMessages = new();
            }
            allMailMessages.Concat(fetcherMailMessages);
        });

        return allMailMessages.ToList();
    }

    private async Task<List<MailMessage>> GetFetcherMailMessages(MailFetcherRequest mailFetcherRequest, MailFetcher<IFetcher> imapFetcher, CancellationToken cancellationToken)
    {
        imapFetcher.ConfigureFetcher(mailFetcherRequest.FetcherConfiguration);
        imapFetcher.ConfigureConnection(mailFetcherRequest.MailServerConnection);
        if (mailFetcherRequest.FetchRequest is not null)
        {
            imapFetcher.ConfigureFetchRequest(mailFetcherRequest.FetchRequest);
        }

        var messages = await imapFetcher.InvokeAsync(cancellationToken);

        return messages;
    }
}
