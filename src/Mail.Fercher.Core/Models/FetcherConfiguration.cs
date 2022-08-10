namespace Mail.Fercher.Core;

public class FetcherConfiguration
{
    /// <summary>
    /// Gets or sets using sequential or parallel
    /// </summary>
    public ParallelismStatus ExecutionType { get; set; } = ParallelismStatus.None;

    /// <summary>
    /// Gets or sets the maximum degree of parallelism enabled by this ParallelOptions instance.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 3;

    /// <summary>
    /// Gets or sets the minumum messages count to enable parallel
    /// </summary>
    public int MinimumMessageToEnableParallel { get; set; } = 1000;

    public enum ParallelismStatus
    {
        /// <summary>
        /// Use sequential
        /// </summary>
        None,
        /// <summary>
        /// Use parallel 
        /// </summary>
        ConditionalParallel,
        /// <summary>
        /// If use it then MinimumMessageToEnableParallel will be ignored and get mail messages in parallel
        /// </summary>
        ForceParallel
    }
}
