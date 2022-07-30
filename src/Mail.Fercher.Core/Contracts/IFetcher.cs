namespace Mail.Fercher.Core;

public interface IFetcher
{
    Task<List<MailMessage>> FetchAsync(MailServerConnection mailServerConnection, FetchRequest fetchRequest, CancellationToken cancellationToken);
}