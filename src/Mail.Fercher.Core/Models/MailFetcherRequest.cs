namespace Mail.Fercher.Core.Models;

public class MailFetcherRequest
{
    public FetcherConfiguration FetcherConfiguration { get; set; }

    public MailServerConnection MailServerConnection { get; set; }

    public FetchRequest? FetchRequest { get; set; }

    public ProtocolType ProtocolType { get; set; }
}

public enum ProtocolType
{
    Imap,
    Pop3
}