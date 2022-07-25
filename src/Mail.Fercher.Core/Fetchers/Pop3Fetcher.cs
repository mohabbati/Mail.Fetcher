namespace Mail.Fercher.Core;

internal class Pop3Fetcher : IFetcher
{
    public Task<List<MailMessage>> Fetch(MailServerConnection mailServerConnection)
    {
        throw new NotImplementedException();
    }
}
