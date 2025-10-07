namespace LifeTimes;

/// <summary>
/// The default implemenation of <see cref="ILifeTimeOptionsConfiguration"/>
/// </summary>
/// <inheritdoc cref="ILifeTimeOptionsConfiguration"/>
internal class LifeTimeOptionsConfiguration : ILifeTimeOptionsConfiguration
{
    private readonly Action<IServiceProvider, LifeTimeOptionsBuilder> _configure;

    public LifeTimeOptionsConfiguration(Action<IServiceProvider, LifeTimeOptionsBuilder> configure)
        => _configure = configure;

    /// <inheritdoc cref="ILifeTimeOptionsConfiguration.Configure(IServiceProvider, LifeTimeOptionsBuilder)"/>
    public void Configure(IServiceProvider serviceProvider, LifeTimeOptionsBuilder optionsBuilder)
        => _configure(serviceProvider, optionsBuilder);
}
