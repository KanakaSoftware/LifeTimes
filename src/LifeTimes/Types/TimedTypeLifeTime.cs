using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

/// <summary>
/// An implementation of <see cref="ITypeLifeTime{T}"/> that provides a timed lifetime for a service instance,
/// using a <see cref="CancellationToken"/> to control expiration.
/// </summary>
internal sealed class TimedTypeLifeTime<T> : ITypeLifeTime<T>, IDisposable where T : class
{
    // The duration after which the service instance expires.
    private readonly TimeSpan _interval;

    // The internal service provider used to create scopes.
    private readonly IServiceProvider _serviceProvider;

    // Synchronization primitive for thread-safe creation and disposal of the service instance.
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    // The current service scope holding the instance.
    private IServiceScope? _serviceScope;

    // The cancellation token source that triggers expiration.
    private CancellationTokenSource? _cts;

    // Registration for the cancellation callback.
    private CancellationTokenRegistration? _ctRegistration;

    private bool _disposed = default;

    public TimedTypeLifeTime(IServiceProvider serviceProvider, TimeSpan interval)
    {
        _interval = interval;
        _serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _lock.EnterWriteLock();
        try
        {
            _ctRegistration?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
            _serviceScope?.Dispose();
        }
        finally
        {
            _lock.ExitWriteLock();
            _lock.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the current lifetime.
    /// </summary>
    /// <returns>The cancellation token for the current lifetime.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the token is not initialized.</exception>
    public CancellationToken GetCancellationToken()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TimedTypeLifeTime<T>));

        _lock.EnterReadLock();
        try
        {
            if (_cts == null)
            {
                throw new InvalidOperationException("CancellationToken not initialized.");
            }
            return _cts.Token;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets the service instance, creating a new one if the previous has expired or no instance exists yet.
    /// </summary>
    /// <returns>The service instance of type <typeparamref name="T"/>.</returns>
    public T GetInstance()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TimedTypeLifeTime<T>));

        bool shouldRecreate = default;

        _lock.EnterReadLock();
        try
        {
            shouldRecreate = _serviceScope == null || _cts!.IsCancellationRequested;
        }
        finally
        {
            _lock.ExitReadLock();
        }

        if (shouldRecreate)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                // Double-check in case another thread already created the scope.
                shouldRecreate = _serviceScope == null || _cts!.IsCancellationRequested;
                if (shouldRecreate)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        // Dispose previous callback and resources.
                        _ctRegistration?.Dispose();
                        _cts?.Cancel();
                        _cts?.Dispose();
                        _serviceScope?.Dispose();

                        // Create a new cancellation token source with the specified interval.
                        _cts = new CancellationTokenSource(_interval);

                        // Create a new service scope for the instance.
                        _serviceScope = _serviceProvider.CreateScope();

                        // Register a callback to clean up resources when the token is cancelled.
                        _ctRegistration = _cts.Token.Register(() =>
                        {
                            _lock.EnterWriteLock();
                            try
                            {
                                // Dispose callback and resources.
                                _ctRegistration?.Dispose();
                                _ctRegistration = null;
                                _cts?.Dispose();
                                _cts = null;
                                _serviceScope?.Dispose();
                                _serviceScope = null;
                            }
                            finally
                            {
                                _lock.ExitWriteLock();
                            }
                        });
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        // Return the required service from the current scope.
        _lock.EnterReadLock();
        try
        {
            // Handle the rare case where the callback disposed the scope after the check above.
            if (_serviceScope == null)
                return GetInstance();

            return _serviceScope!.ServiceProvider.GetRequiredService<T>();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
