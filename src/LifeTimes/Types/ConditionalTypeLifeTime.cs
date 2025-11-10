using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using IAsyncDisposable = System.IAsyncDisposable;

namespace LifeTimes.Types;

/// <summary>
/// An implementation of <see cref="ITypeLifeTime{T}"/> that provides a conditional lifetime for a service instance.
/// </summary>
internal sealed class ConditionalTypeLifeTime<T> : ITypeLifeTime<T>, IDisposable, IAsyncDisposable where T : class, IConditional
{
    // The internal service provider used to create scopes.
    private readonly IServiceProvider _serviceProvider;

    // Synchronization primitive for thread-safe creation and disposal of the service instance.
#pragma warning disable VSTHRD012 // Provide JoinableTaskFactory where allowed
    private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();
#pragma warning restore VSTHRD012 // Provide JoinableTaskFactory where allowed

    // The current service scope holding the instance.
    private AsyncServiceScope? _serviceScope;

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

        _lock.Dispose();
        _cts?.Dispose();
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
    /// <returns>A ValueTask that represents the service instance of type <typeparamref name="T"/>.</returns>
    public async ValueTask<T> GetInstanceAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConditionalTypeLifeTime<T>));

        cancellationToken.ThrowIfCancellationRequested();

        T? instance;

        // Fast read: check if instance exists and if condition passes
        using (await _lock.ReadLockAsync(cancellationToken))
        {
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance != null && !await instance.ConditionAsync(cancellationToken))
            {
                return instance;
            }
        }

        // Slow path: recreate the scope if instance is null or condition fails
        using (await _lock.UpgradeableReadLockAsync(cancellationToken))
        {
            // Double-check inside upgradeable lock
            instance = _serviceScope?.ServiceProvider.GetService<T>();
            if (instance == null || await instance.ConditionAsync(cancellationToken))
            {
                using (await _lock.WriteLockAsync(cancellationToken))
                {
                    // Dispose previous resources
                    if (_cts != null)
                    {
                        await _cts.CancelAsync();
                        _cts.Dispose();
                    }
                    if (_serviceScope is { } scope)
                        await scope.DisposeAsync();

                    // Create a new scope and cancellation token
                    _serviceScope = _serviceProvider.CreateAsyncScope();
                    _cts = new CancellationTokenSource();
                }
            }
        }

        // Return the required service from the current (new) scope
        using (await _lock.ReadLockAsync(cancellationToken))
        {
            if (_serviceScope is { } scope)
                return scope.ServiceProvider.GetRequiredService<T>();

            throw new InvalidOperationException("This path should be unreachable.");
        }
    }
}
