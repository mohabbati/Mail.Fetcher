namespace Mail.Fercher.Core;

public class MailFetcher<TFetcher>
    where TFetcher : IFetcher
{
    private readonly TFetcher fetcher;

    private readonly MailServerConnection _mailServerConnection;
    private readonly FetcherConfiguration _fetcherConfiguration;

    public MailFetcher(MailServerConnection mailServerConnection, FetcherConfiguration fetcherConfiguration)
    {
        ArgumentNullException.ThrowIfNull(mailServerConnection);
        ArgumentNullException.ThrowIfNull(fetcherConfiguration);

        _mailServerConnection = mailServerConnection;
        _fetcherConfiguration = fetcherConfiguration;
    }

    public async Task<List<MailMessage>> InvokeAsync(CancellationToken cancellationToken)
    {
        await OnFetching(fetcher, cancellationToken);

        List<MailMessage> result;

        try
        {
            if (_fetcherConfiguration.ExecutionType == FetcherConfiguration.ParallelismStatus.None)
            {
                result = await fetcher.FetchAsync(_mailServerConnection, cancellationToken);
            }
            else if (_fetcherConfiguration.ExecutionType is FetcherConfiguration.ParallelismStatus.ConditionalParallel or FetcherConfiguration.ParallelismStatus.ForceParallel)
            {
                result = await fetcher.FetchParallelAsync(_fetcherConfiguration, _mailServerConnection, cancellationToken);
            }
            else
            {
                result = new List<MailMessage>();
            }
        }
        catch (Exception)
        {
            await OnFetchFailed(fetcher, cancellationToken);

            throw;
        }

        await OnFetched(fetcher, cancellationToken);

        return result;
    }

    /// <summary>
    /// Gets called right before Fetch
    /// </summary>
    protected virtual Task OnFetching(TFetcher fetcher, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets called right after Fetch
    /// </summary>
    protected virtual Task OnFetched(TFetcher fetcher, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets called right after Fetch failed
    /// </summary>
    protected virtual Task OnFetchFailed(TFetcher fetcher, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}