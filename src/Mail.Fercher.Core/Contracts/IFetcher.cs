namespace Mail.Fercher.Core;

public interface IFetcher
{
    Task<List<MailMessage>> FetchAsync(MailServerConnection mailServerConnection, CancellationToken cancellationToken);

    Task<List<MailMessage>> FetchParallelAsync(FetcherConfiguration fetcherConfiguration, MailServerConnection mailServerConnection, CancellationToken cancellationToken);
}