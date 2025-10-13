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

        var cts = _cts;
        if (cts == null)
            throw new InvalidOperationException("CancellationToken not initialized. Call GetInstance() first.");

        return cts.Token;
    }


    /// <summary>
    /// Gets the service instance, creating a new one if the previous has expired or no instance exists yet.
    /// </summary>
    /// <returns>The service instance of type <typeparamref name="T"/>.</returns>
    public T GetInstance()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TimedTypeLifeTime<T>));

        T? instance;

        // Fast path: read lock only, returns if instance exists and token not expired
        _lock.EnterReadLock();
        try
        {
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance != null && !_cts!.IsCancellationRequested)
            {
                return instance;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // Slow path: upgradeable read lock to safely recreate scope
        _lock.EnterUpgradeableReadLock();
        try
        {
            // Double-check inside lock
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance == null || _cts!.IsCancellationRequested)
            {
                _lock.EnterWriteLock();
                try
                {
                    // Dispose previous resources
                    _ctRegistration?.Dispose();
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _serviceScope?.Dispose();

                    // Create new cancellation token and scope
                    _cts = new CancellationTokenSource(_interval);
                    _serviceScope = _serviceProvider.CreateScope();

                    // Register callback to clean up when token expires
                    _ctRegistration = _cts.Token.Register(() =>
                    {
                        _lock.EnterWriteLock();
                        try
                        {
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

        // Return instance from current scope (guaranteed to exist here)
        return _serviceScope!.ServiceProvider.GetRequiredService<T>();
    }
}
