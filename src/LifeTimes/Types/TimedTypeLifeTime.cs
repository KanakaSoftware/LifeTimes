using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using IAsyncDisposable = System.IAsyncDisposable;

namespace LifeTimes.Types;

/// <summary>
/// An implementation of <see cref="ITypeLifeTime{T}"/> that provides a timed lifetime for a service instance,
/// using a <see cref="CancellationToken"/> to control expiration.
/// </summary>
internal sealed class TimedTypeLifeTime<T> : ITypeLifeTime<T>, IDisposable, IAsyncDisposable where T : class
{
    // The duration after which the service instance expires.
    private readonly TimeSpan _interval;

    // The internal service provider used to create scopes.
    private readonly IServiceProvider _serviceProvider;

    // Synchronization primitive for thread-safe creation and disposal of the service instance.
#pragma warning disable VSTHRD012 // Provide JoinableTaskFactory where allowed
    private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();
#pragma warning restore VSTHRD012 // Provide JoinableTaskFactory where allowed

    // The current service scope holding the instance.
    private AsyncServiceScope? _serviceScope;

    // The cancellation token source that triggers expiration.
    private CancellationTokenSource? _cts;

    // Registration for the cancellation callback.
    private CancellationTokenRegistration _ctRegistration = default;

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

        _lock.Dispose();
        _cts?.Dispose();
        _ctRegistration.Dispose();
        if (_serviceScope is { } scope)
            scope.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _lock.Dispose();
        _cts?.Dispose();
        await _ctRegistration.DisposeAsync();
        if (_serviceScope is { } scope)
            await scope.DisposeAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
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
    public async ValueTask<T> GetInstanceAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TimedTypeLifeTime<T>));

        cancellationToken.ThrowIfCancellationRequested();

        T? instance;

        // Fast path: read lock only, returns if instance exists and token not expired
        using (await _lock.ReadLockAsync(cancellationToken))
        {
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance != null && !_cts!.IsCancellationRequested)
            {
                return instance;
            }
        }

        // Slow path: upgradeable read lock to safely recreate scope
        using (await _lock.UpgradeableReadLockAsync(cancellationToken))
        {
            // Double-check inside lock
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance == null || _cts!.IsCancellationRequested)
            {
                using (await _lock.WriteLockAsync(cancellationToken))
                {
                    // Dispose previous resources
                    await _ctRegistration.DisposeAsync();
                    if (_cts != null)
                    {
                        await _cts.CancelAsync();
                        _cts.Dispose();
                    }
                    if (_serviceScope is { } scope)
                        await scope.DisposeAsync();

                    // Create new cancellation token and scope
                    _cts = new CancellationTokenSource(_interval);
                    _serviceScope = _serviceProvider.CreateAsyncScope();

                    // Register callback to clean up when token expires
                    _ctRegistration = _cts.Token.Register(() =>
                    {
                        _ = Task.Run(async () =>
                        {
                            using (await _lock.WriteLockAsync())
                            {
                                await _ctRegistration.DisposeAsync();
                                _ctRegistration = default;
                                _cts?.Dispose();
                                _cts = null;
                                if (_serviceScope is { } scope)
                                    await scope.DisposeAsync();
                                _serviceScope = null;
                            }
                        });
                    });
                }
            }
        }

        // Return instance from current scope
        using (await _lock.ReadLockAsync(cancellationToken))
        {
            if (_serviceScope is { } scope)
                return scope.ServiceProvider.GetRequiredService<T>();

            // Handle the rare case where the callback disposed the scope after the check above.
            return await GetInstanceAsync();
        }
    }
}
