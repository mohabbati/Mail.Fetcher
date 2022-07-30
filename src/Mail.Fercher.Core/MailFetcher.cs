using MailKit.Search;

namespace Mail.Fercher.Core;

public class MailFetcher
{
    public async Task<List<MailMessage>> GetAsync(IFetcher fetcher, MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        return await this.GetAsync(fetcher, mailServerConnection, new() { Query = SearchQuery.All }, cancellationToken);
    }

    public async Task<List<MailMessage>> GetAsync(IFetcher fetcher, MailServerConnection mailServerConnection, FetchRequest fetchRequest, CancellationToken cancellationToken)
    {
        var result = await fetcher.FetchAsync(mailServerConnection, fetchRequest, cancellationToken);

        return result;
    }
}
