using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes;

/// <summary>
/// Contains configuration options for service lifetimes.
/// </summary>
public sealed class LifeTimeOptions
{
    /// <value>
    /// Gets or sets the <see cref="IServiceCollection"/>.
    /// </value>
    public Lazy<IServiceCollection> ServiceCollection { get; set; } = new Lazy<IServiceCollection>(() => new ServiceCollection());

    public LifeTimeOptions()
    {
    }
}
