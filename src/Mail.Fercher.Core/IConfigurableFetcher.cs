namespace Mail.Fercher.Core;

public interface IConfigurableFetcher
{
    void SetFetchRequest(FetchRequest fetchRequest);
}