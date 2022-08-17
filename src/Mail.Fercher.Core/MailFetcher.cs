namespace Mail.Fercher.Core;

public class MailFetcher<TFetcher>
    where TFetcher : IFetcher
{
    private readonly TFetcher _fetcher;
    private readonly FetcherConfiguration _fetcherConfiguration;
    private MailServerConnection _mailServerConnection;

    public MailFetcher(TFetcher fetcher, FetcherConfiguration fetcherConfiguration)
    {
        ArgumentNullException.ThrowIfNull(fetcher);
        ArgumentNullException.ThrowIfNull(fetcherConfiguration);

        _fetcher = fetcher;
        _fetcherConfiguration = fetcherConfiguration;
    }

    public void ConfigureConnection(MailServerConnection mailServerConnection)
    {
        ArgumentNullException.ThrowIfNull(mailServerConnection);

        _mailServerConnection = mailServerConnection;
    }

    public void ConfigureFetchRequest(FetchRequest fetchRequest)
    {
        ArgumentNullException.ThrowIfNull(fetchRequest);

        if (_fetcher is IConfigurableFetcher configurabelFetcher)
        {
            configurabelFetcher.SetFetchRequest(fetchRequest);
        }
        else
        {
            throw new InvalidOperationException("The fetcher is not configurable.");
        }
    }

    public async Task<List<MailMessage>> InvokeAsync(CancellationToken cancellationToken)
    {
        await OnFetching(_fetcher, cancellationToken);

        List<MailMessage> result;

        try
        {
            if (_fetcherConfiguration.ExecutionType == FetcherConfiguration.ParallelismStatus.None)
            {
                result = await _fetcher.FetchAsync(_mailServerConnection, cancellationToken);
            }
            else if (_fetcherConfiguration.ExecutionType is FetcherConfiguration.ParallelismStatus.ConditionalParallel or FetcherConfiguration.ParallelismStatus.ForceParallel)
            {
                result = await _fetcher.FetchParallelAsync(_fetcherConfiguration, _mailServerConnection, cancellationToken);
            }
            else
            {
                result = new List<MailMessage>();
            }
        }
        catch (Exception)
        {
            await OnFetchFailed(_fetcher, cancellationToken);

            throw;
        }

        await OnFetched(_fetcher, cancellationToken);

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