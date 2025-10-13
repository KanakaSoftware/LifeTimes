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

        // Fast read without lock
        var cts = _cts;
        if (cts == null)
            throw new InvalidOperationException(
                "CancellationToken not initialized. Call GetInstance() first to initialize the instance.");

        return cts.Token;
    }


    /// <summary>
    /// Gets the service instance, creating a new one if condition is true or no instance exists yet.
    /// </summary>
    /// <returns>The service instance of type <typeparamref name="T"/>.</returns>
    public T GetInstance()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConditionalTypeLifeTime<T>));

        T? instance;

        // Fast read: check if instance exists and if condition passes
        _lock.EnterReadLock();
        try
        {
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance != null && !instance.Condition())
            {
                return instance;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // Slow path: recreate the scope if instance is null or condition fails
        _lock.EnterUpgradeableReadLock();
        try
        {
            // Double-check inside upgradeable lock
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance == null || instance.Condition())
            {
                _lock.EnterWriteLock();
                try
                {
                    // Dispose previous resources
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _serviceScope?.Dispose();

                    // Create a new scope and cancellation token
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

        // Return the required service from the current (new) scope
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
