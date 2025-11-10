namespace LifeTimes;

/// <summary>
/// Defines a contract for managing the lifetime of service objects.
/// </summary>
/// <typeparam name="T">The type of the service object.</typeparam>
public interface ITypeLifeTime<T> where T : class
{
    /// <summary>
    /// Retrieves an instance of the service object of type <typeparamref name="T"/>
    /// according to the configured lifetime policy asynchronously (e.g. timed, conditional).
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns> A ValueTask that represents an instance of type <typeparamref name="T"/>.
    /// </returns>
    public ValueTask<T> GetInstanceAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Retrieves the <see cref="CancellationToken"/> associated with the service object
    /// of type <typeparamref name="T"/> for the current lifetime scope.
    /// </summary>
    /// <returns>
    /// A <see cref="CancellationToken"/> for the service object of type <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if the <see cref="CancellationToken"/> has not been initialized for the current lifetime scope.
    /// </exception>
    public CancellationToken GetCancellationToken();
}
