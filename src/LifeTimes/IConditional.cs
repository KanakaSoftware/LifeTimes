namespace LifeTimes;

/// <summary>
/// Defines a contract for a condition that determines
/// whether a service object should be recreated.
/// </summary>
public interface IConditional
{
    /// <summary>
    /// The condition for recreating the service object.
    /// </summary>
    /// <returns>
    /// <see cref="true"/> if the service object should be recreated;
    /// <see cref="false"/> if the existing instance can be reused.
    /// </returns>
    bool Condition();
}
