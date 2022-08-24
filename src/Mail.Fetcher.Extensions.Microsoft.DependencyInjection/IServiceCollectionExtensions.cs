using Mail.Fercher;
using Microsoft.Extensions.DependencyInjection;

namespace Mail.Fetcher.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// It registers Imap mail fetcher
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddImapFetcher(this IServiceCollection services)
    {
        return services.AddScoped(typeof(MailFetcher<ImapFetcher>), serviceProvider =>
        {
            var imapFetcher = new ImapFetcher();
            var mailFetcher = new MailFetcher<ImapFetcher>(imapFetcher);

            return mailFetcher;
        });
    }

    /// <summary>
    /// It registers Pop3 mail fetcher
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddPop3Fetcher(this IServiceCollection services)
    {
        return services.AddScoped(typeof(MailFetcher<Pop3Fetcher>), serviceProvider =>
        {
            var pop3Fetcher = new Pop3Fetcher();
            var mailFetcher = new MailFetcher<Pop3Fetcher>(pop3Fetcher);

            return mailFetcher;
        });
    }

    /// <summary>
    /// It registers all implementions of IFetcher
    /// </summary>
    /// <param name="services"></param>
    public static void AddMailFetchers(this IServiceCollection services)
    {
        AddImapFetcher(services);
        AddPop3Fetcher(services);
    }

    public static void AddMailFetcherService(this IServiceCollection services)
    {
        AddMailFetchers(services);

        services.AddScoped(typeof(MailFetcherService), serviceProvider =>
        {
            var mailFetcherService = new MailFetcherService(serviceProvider);

            return mailFetcherService;
        });
    }
}