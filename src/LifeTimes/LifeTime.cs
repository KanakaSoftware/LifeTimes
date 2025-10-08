using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes;

/// <summary>
/// The <see cref="ILifeTime"/> implementation using an internal DI container.
/// </summary>
/// <inheritdoc cref="ILifeTime"/>
internal sealed class LifeTime : ILifeTime, IDisposable
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
        var buidler = new LifeTimeOptionsBuilder();

        // Retrieve the configuration for options from the root service provider
        var configuration = rootServiceProvider.GetRequiredService<ILifeTimeOptionsConfiguration>();

        // Configure the builder using the retrieved configuration
        configuration.Configure(rootServiceProvider, buidler);

        // Build the internal service provider using the configured options
        var options = buidler.Options;
        _serviceProvider = options.ServiceCollection.Value.BuildServiceProvider();
    }

    /// <inheritdoc cref="ILifeTime.GetService{T}"/>
    public T? GetService<T>() where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LifeTime));

        return _serviceProvider.GetService<ITypeLifeTime<T>>()?.GetInstance();
    }

    /// <inheritdoc cref="ILifeTime.GetRequiredService{T}"/>
    public T GetRequiredService<T>() where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LifeTime));

        return _serviceProvider.GetRequiredService<ITypeLifeTime<T>>().GetInstance();
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
    }
}
