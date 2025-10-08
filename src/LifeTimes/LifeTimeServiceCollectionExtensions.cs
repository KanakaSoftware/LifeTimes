using LifeTimes;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering LifeTime services in the IServiceCollection.
/// </summary>
public static class LifeTimeServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="ILifeTime"/> service to the IServiceCollection with configuration.
    /// </summary>
    /// <param name="serviceCollection">The IServiceCollection to add services to.</param>
    /// <param name="optionAction">
    /// An action to configure LifeTime options using the service provider and options builder.
    /// </param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddLifeTime(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, LifeTimeOptionsBuilder> optionAction)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(optionAction);

        serviceCollection.ConfigureLifeTime(optionAction);
        serviceCollection.AddSingleton<ILifeTime, LifeTime>();

        return serviceCollection;
    }

    private static IServiceCollection ConfigureLifeTime(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, LifeTimeOptionsBuilder> optionAction)
    {
        serviceCollection.AddSingleton<ILifeTimeOptionsConfiguration>(
            p => new LifeTimeOptionsConfiguration(optionAction));

        return serviceCollection;
    }
}
