using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

/// <summary>
/// An implementation of <see cref="ITypeLifeTime{T}"/> that provides a conditional lifetime for a service instance.
/// </summary>
internal sealed class ConditionalTypeLifeTime<T> : ITypeLifeTime<T>, IDisposable where T : class, IConditional
{
    // The internal service provider used to create scopes.
    private readonly IServiceProvider _serviceProvider;

    // Synchronization primitive for thread-safe creation and disposal of the service instance.
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    // The current service scope holding the instance.
    private IServiceScope? _serviceScope;

    // The cancellation token source.
    private CancellationTokenSource? _cts;

    private bool _disposed = default;

    public ConditionalTypeLifeTime(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _lock.EnterWriteLock();
        try
        {
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
            throw new ObjectDisposedException(nameof(ConditionalTypeLifeTime<T>));

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
    /// Gets the service instance, creating a new one if condition is true or no instance exists yet.
    /// </summary>
    /// <returns>The service instance of type <typeparamref name="T"/>.</returns>
    public T GetInstance()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConditionalTypeLifeTime<T>));

        bool shouldRecreate = default;
        T? instance;

        _lock.EnterReadLock();
        try
        {
            instance = _serviceScope?.ServiceProvider.GetRequiredService<T>();
            shouldRecreate = instance == null || instance.Condition();
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // If the scope needs to be recreated, proceed with an upgradeable lock.
        if (shouldRecreate)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                // Double-check in case another thread already created the scope.
                instance = _serviceScope?.ServiceProvider.GetRequiredService<T>();
                shouldRecreate = instance == null || instance.Condition();
                if (shouldRecreate)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        // Dispose resources
                        _cts?.Cancel();
                        _cts?.Dispose();
                        _serviceScope?.Dispose();

                        // Create a new service scope and cancellation token source.
                        _serviceScope = _serviceProvider.CreateScope();
                        _cts = new CancellationTokenSource();
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

        // Return the required service from the current scope
        _lock.EnterReadLock();
        try
        {
            return _serviceScope!.ServiceProvider.GetRequiredService<T>();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
