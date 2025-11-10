using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes;

/// <summary>
/// The <see cref="ILifeTime"/> implementation using an internal DI container.
/// </summary>
/// <inheritdoc cref="ILifeTime"/>
internal sealed class LifeTime : ILifeTime, IDisposable, IAsyncDisposable
{
    // Internal service provider
    private readonly ServiceProvider _serviceProvider;

    private bool _disposed = default;

    /// <summary>
    /// Constructs a new LifeTime instance, configuring its internal service provider
    /// </summary>
    /// <param name="rootServiceProvider">The root service provider from application DI container.</param>
    public LifeTime(IServiceProvider rootServiceProvider)
    {
        var builder = new LifeTimeOptionsBuilder();

        // Retrieve the configuration for options from the root service provider
        var configuration = rootServiceProvider.GetRequiredService<ILifeTimeOptionsConfiguration>();

        // Configure the builder using the retrieved configuration
        configuration.Configure(rootServiceProvider, builder);

        // Build the internal service provider using the configured options
        var options = builder.Options;
        _serviceProvider = options.ServiceCollection.Value.BuildServiceProvider();
    }

    /// <inheritdoc cref="ILifeTime.GetServiceAsync{T}"/>
    public ValueTask<T?> GetServiceAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LifeTime));

        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled<T?>(cancellationToken);

        var serviceLifeTime = _serviceProvider.GetService<ITypeLifeTime<T>>();
        if (serviceLifeTime == null)
            return ValueTask.FromResult<T?>(null);

        return WrapGetInstanceAsync(serviceLifeTime.GetInstanceAsync(cancellationToken));

        static async ValueTask<T?> WrapGetInstanceAsync(ValueTask<T> getInstanceAsyncTask)
        {
            return await getInstanceAsyncTask;
        }
    }

    /// <inheritdoc cref="ILifeTime.GetRequiredServiceAsync{T}"/>
    public ValueTask<T> GetRequiredServiceAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LifeTime));

        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled<T>(cancellationToken);

        return _serviceProvider.GetRequiredService<ITypeLifeTime<T>>().GetInstanceAsync(cancellationToken);
    }

    /// <inheritdoc cref="ILifeTime.GetCancellationToken{T}"/>
    public CancellationToken GetCancellationToken<T>() where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LifeTime));

        return _serviceProvider.GetRequiredService<ITypeLifeTime<T>>().GetCancellationToken();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _serviceProvider.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_serviceProvider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else
#pragma warning disable VSTHRD103 // Call async methods when in an async method
            _serviceProvider.Dispose();
#pragma warning restore VSTHRD103 // Call async methods when in an async method

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
