namespace Mail.Fercher.Core;

internal class ImapFetcher : IFetcher
{
    public Task<List<MailMessage>> Fetch(MailServerConnection mailServerConnection)
    {
        throw new NotImplementedException();
    }
}
