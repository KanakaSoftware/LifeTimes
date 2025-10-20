using LifeTimes.Types;
using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes;

/// <summary>
/// Builder class for configuring <see cref="LifeTimeOptions"/>.
/// </summary>
public class LifeTimeOptionsBuilder
{
    private readonly LifeTimeOptions _options;

    /// <summary>
    /// Gets the configured <see cref="LifeTimeOptions"/>.
    /// </summary>
    public LifeTimeOptions Options => _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LifeTimeOptionsBuilder"/> class with default options.
    /// </summary>
    public LifeTimeOptionsBuilder() : this(new LifeTimeOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LifeTimeOptionsBuilder"/> class with the specified options.
    /// </summary>
    /// <param name="options">The <see cref="LifeTimeOptions"/> to use.</param>
    public LifeTimeOptionsBuilder(LifeTimeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets the <see cref="IServiceCollection"/> to be used by the options.
    /// </summary>
    /// <param name="serviceCollection">The service collection to use.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder UseServiceCollection(IServiceCollection serviceCollection)
    {
        _options.ServiceCollection = new Lazy<IServiceCollection>(() => serviceCollection);
        return this;
    }

    /// <summary>
    /// Adds a timed lifetime for the specified type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="serviceProvider">The service provider used to create instances of <typeparamref name="TService"/>.</param>
    /// <param name="interval">The time interval for the timed lifetime.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddTimed<TService>(IServiceProvider serviceProvider, TimeSpan interval)
        where TService : class
    {
        return AddTimed<TService, TService>(serviceProvider, interval);
    }

    /// <summary>
    /// Adds a timed lifetime for the specified type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="serviceProvider">The service provider used to create instances of <typeparamref name="TService"/>.</param>
    /// <param name="interval">The time interval for the timed lifetime.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddTimed<TService, TImplementation>(IServiceProvider serviceProvider, TimeSpan interval)
        where TService : class
        where TImplementation : class, TService
    {
        return AddTimed<TService, TImplementation>(() => ActivatorUtilities.CreateInstance<TImplementation>(serviceProvider), interval);
    }

    /// <summary>
    /// Adds a timed lifetime for the specified type <typeparamref name="TService"/> using the provided factory method.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="factory">A factory delegate to create instances of <typeparamref name="TService"/>.</param>
    /// <param name="interval">The time interval for the timed lifetime.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddTimed<TService>(Func<TService> factory, TimeSpan interval)
        where TService : class
    {
        return AddTimed<TService, TService>(factory, interval);
    }


    /// <summary>
    /// Adds a timed lifetime for the specified type <typeparamref name="TService"/> using the provided factory method.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="factory">A factory delegate to create instances of <typeparamref name="TService"/>.</param>
    /// <param name="interval">The time interval for the timed lifetime.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddTimed<TService, TImplementation>(Func<TImplementation> factory, TimeSpan interval)
        where TService : class
        where TImplementation: class, TService
    {
        _options.ServiceCollection.Value.AddSingleton<ITypeLifeTime<TService>>(serviceProvider => new TimedTypeLifeTime<TService>(serviceProvider, interval));
        _options.ServiceCollection.Value.AddScoped<TService>(serviceProvider => factory());
        return this;
    }

    /// <summary>
    /// Adds a conditional lifetime for the specified type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="serviceProvider">The service provider used to create instances of <typeparamref name="TService"/>.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddConditional<TService>(IServiceProvider serviceProvider)
        where TService : class, IConditional
    {
        return AddConditional<TService,TService>(serviceProvider);
    }

    /// <summary>
    /// Adds a conditional lifetime for the specified type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="serviceProvider">The service provider used to create instances of <typeparamref name="TService"/>.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddConditional<TService, TImplementation>(IServiceProvider serviceProvider)
        where TService : class, IConditional
        where TImplementation: class, TService
    {
        return AddConditional<TService, TImplementation>(() => ActivatorUtilities.CreateInstance<TImplementation>(serviceProvider));
    }


    /// <summary>
    /// Adds a conditional lifetime for the specified type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="factory">A factory delegate to create instances of <typeparamref name="TService"/>.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddConditional<TService>(Func<TService> factory)
        where TService : class, IConditional
    {
        return AddConditional<TService, TService>(factory);
    }

    /// <summary>
    /// Adds a conditional lifetime for the specified type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="factory">A factory delegate to create instances of <typeparamref name="TService"/>.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddConditional<TService, TImplementation>(Func<TImplementation> factory)
        where TService : class, IConditional
        where TImplementation : class, TService
    {
        _options.ServiceCollection.Value.AddSingleton<ITypeLifeTime<TService>>(serviceProvider => new ConditionalTypeLifeTime<TService>(serviceProvider));
        _options.ServiceCollection.Value.AddScoped<TService>(serviceProvider => factory());
        return this;
    }
}
