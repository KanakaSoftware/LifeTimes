namespace LifeTimes;

/// <summary>
/// Defines a contract for configuring <see cref="LifeTimeOptionsBuilder"/> instance
/// using provided <see cref="IServiceProvider"/>.
/// </summary>
/// <param name="serviceProvider">
/// The <see cref="IServiceProvider"/> instance to use.
/// during configuration.
/// </param>
/// <param name="optionsBuilder">
/// The <see cref="LifeTimeOptionsBuilder"/> to be configured.
/// </param>
public interface ILifeTimeOptionsConfiguration
{
    void Configure(IServiceProvider serviceProvider, LifeTimeOptionsBuilder optionsBuilder);
}
