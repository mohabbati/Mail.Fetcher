namespace Mail.Fercher.Core;

public class MailFetcher<TFetcher>
    where TFetcher : IFetcher
{
    private readonly TFetcher _fetcher;
    private FetcherConfiguration _fetcherConfiguration;
    private MailServerConnection _mailServerConnection;

    public MailFetcher(TFetcher fetcher)
    {
        ArgumentNullException.ThrowIfNull(fetcher);

        _fetcher = fetcher;
    }

    /// <summary>
    /// Configure fetcher how to fetch messages, sequential or parallel. If do not set it, the fetcher uses the default values.
    /// </summary>
    /// <param name="fetcherConfiguration"></param>
    /// <returns></returns>
    public MailFetcher<TFetcher> ConfigureFetcher(FetcherConfiguration fetcherConfiguration)
    {
        ArgumentNullException.ThrowIfNull(fetcherConfiguration);

        _fetcherConfiguration = fetcherConfiguration;

        return this;
    }

    /// <summary>
    /// Configure mail server connection. It is required for fetching messages.
    /// </summary>
    /// <param name="mailServerConnection"></param>
    public void ConfigureConnection(MailServerConnection mailServerConnection)
    {
        ArgumentNullException.ThrowIfNull(mailServerConnection);

        _mailServerConnection = mailServerConnection;
    }

    /// <summary>
    /// Configure fetch request to IConfigurableFetcher, such as ImapFetcher.
    /// </summary>
    /// <param name="fetchRequest"></param>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Fetch mail messages from the server based on all the configation.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<MailMessage>> InvokeAsync(CancellationToken cancellationToken)
    {
        if (_mailServerConnection is null)
        {
            throw new Exception($"Configure mail server connection using {nameof(ConfigureConnection)}() method.");
        }

        if (_fetcherConfiguration is null)
        {
            _fetcherConfiguration = new FetcherConfiguration(); //Use the default values
        }

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