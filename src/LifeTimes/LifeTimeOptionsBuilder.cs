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
    /// Adds a timed lifetime for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="serviceProvider">The service provider used to create instances of <typeparamref name="T"/>.</param>
    /// <param name="interval">The time interval for the timed lifetime.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddTimed<T>(IServiceProvider serviceProvider, TimeSpan interval) where T : class
    {
        return AddTimed<T>(() => ActivatorUtilities.CreateInstance<T>(serviceProvider), interval);
    }

    /// <summary>
    /// Adds a timed lifetime for the specified type <typeparamref name="T"/> using the provided factory method.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="factory">A factory delegate to create instances of <typeparamref name="T"/>.</param>
    /// <param name="interval">The time interval for the timed lifetime.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddTimed<T>(Func<T> factory, TimeSpan interval) where T : class
    {
        _options.ServiceCollection.Value.AddSingleton<ITypeLifeTime<T>>(serviceProvider => new TimedTypeLifeTime<T>(serviceProvider, interval));
        _options.ServiceCollection.Value.AddScoped<T>(serviceProvider => factory());
        return this;
    }

    /// <summary>
    /// Adds a conditional lifetime for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="serviceProvider">The service provider used to create instances of <typeparamref name="T"/>.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddConditional<T>(IServiceProvider serviceProvider)
        where T : class, IConditional
    {
        return AddConditional<T>(() => ActivatorUtilities.CreateInstance<T>(serviceProvider));
    }

    /// <summary>
    /// Adds a conditional lifetime for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="factory">A factory delegate to create instances of <typeparamref name="T"/>.</param>
    /// <returns>The current <see cref="LifeTimeOptionsBuilder"/> instance.</returns>
    public LifeTimeOptionsBuilder AddConditional<T>(Func<T> factory)
        where T : class, IConditional
    {
        _options.ServiceCollection.Value.AddSingleton<ITypeLifeTime<T>>(serviceProvider => new ConditionalTypeLifeTime<T>(serviceProvider));
        _options.ServiceCollection.Value.AddScoped<T>(serviceProvider => factory());
        return this;
    }
}
