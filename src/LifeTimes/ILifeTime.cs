namespace LifeTimes;

/// <summary>
/// Defines a contract for managing the lifetime of service objects.
/// </summary>
public interface ILifeTime
{
    /// <summary>
    /// Retrieves a service object of the specified type, or <see langword="null"/> if it is not available.
    /// </summary>
    /// <typeparam name="T">The type of the service object to retrieve.</typeparam>
    /// <returns>
    /// An instance of <typeparamref name="T"/> if available; otherwise, <see langword="null"/>.
    /// </returns>
    /// <seealso cref="GetRequiredService{T}"/>
    public T? GetService<T>() where T : class;

    /// <summary>
    /// Retrieves a service object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service object to retrieve.</typeparam>
    /// <returns>
    /// An instance of <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if no service object of type <typeparamref name="T"/> is found.
    /// </exception>
    /// <seealso cref="GetService{T}"/>
    public T GetRequiredService<T>() where T : class;

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the service object of the specified type.
    /// </summary>
    /// <remarks>
    /// This is useful when the service object implements <see cref="IDisposable"/> or requires cancellation support.
    /// </remarks>
    /// <typeparam name="T">The type of the service object whose <see cref="CancellationToken"/> is requested.</typeparam>
    /// <returns>
    /// The <see cref="CancellationToken"/> associated with the service object of type <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if no service object of type <typeparamref name="T"/> is found, or if the <see cref="CancellationToken"/> is not initialized.
    /// </exception>
    public CancellationToken GetCancellationToken<T>() where T : class;
}
