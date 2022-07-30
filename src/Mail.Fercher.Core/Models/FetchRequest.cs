using MailKit.Search;

namespace Mail.Fercher.Core;

public class FetchRequest
{
    public SearchQuery? Query { get; set; }
}
