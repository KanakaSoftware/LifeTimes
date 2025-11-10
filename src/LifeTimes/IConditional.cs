namespace LifeTimes;

/// <summary>
/// Defines a contract for a condition that determines
/// whether a service object should be recreated.
/// </summary>
public interface IConditional
{
    /// <summary>
    /// The condition for recreating the service object asynchronously.
    /// </summary>
    /// <returns>
    /// A ValueTask that represents
    /// <see cref="true"/> if the service object should be recreated;
    /// <see cref="false"/> if the existing instance can be reused.
    /// </returns>
    ValueTask<bool> ConditionAsync(CancellationToken cancellationToken);
}
