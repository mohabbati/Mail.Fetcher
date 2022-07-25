namespace Mail.Fercher.Core;

public interface IFetcher
{
    Task<List<MailMessage>> Fetch(MailServerConnection mailServerConnection);
}