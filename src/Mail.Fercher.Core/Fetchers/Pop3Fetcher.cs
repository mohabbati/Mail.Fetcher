namespace Mail.Fercher.Core;

public class Pop3Fetcher : IFetcher
{
    public Task<List<MailMessage>> FetchAsync(MailServerConnection mailServerConnection, FetchRequest fetchRequest, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
