namespace Mail.Fercher.Core;

public class MailFetcher<TFetcher>
    where TFetcher : IFetcher
{
    private readonly TFetcher fetcher;

    private readonly FetcherConfiguration _fetcherConfiguration;

    public MailFetcher()
    {
        _fetcherConfiguration = new FetcherConfiguration();
    }

    public MailFetcher(FetcherConfiguration fetcherConfiguration)
    {
        _fetcherConfiguration = fetcherConfiguration;
    }

    public async Task<List<MailMessage>> InvokeAsync(MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        await OnFetching(fetcher, cancellationToken);

        var result = new List<MailMessage>();

        try
        {
            if (_fetcherConfiguration.ExecutionType == FetcherConfiguration.ParallelismStatus.None)
            {
                result = await fetcher.FetchAsync(mailServerConnection, cancellationToken);
            }
            else if (_fetcherConfiguration.ExecutionType is FetcherConfiguration.ParallelismStatus.ConditionalParallel or FetcherConfiguration.ParallelismStatus.ForceParallel)
            {
                result = await fetcher.FetchParallelAsync(_fetcherConfiguration, mailServerConnection, cancellationToken);
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