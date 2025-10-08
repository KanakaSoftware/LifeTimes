using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

public class ILifeTimeTest
{
    private class TestService
    {
    }

    private class TestServiceDisposable : IDisposable
    {
        public bool _disposed = false;

        public void Dispose() => _disposed = true;
    }

    [Fact]
    public void GetService_ReturnsNull()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) => { });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = lifetime.GetService<TestService>();

        // assert
        Assert.Null(instance);
    }

    [Fact]
    public void GetRequiredService_ThrowsInvalidOperationException()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) => { });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        Assert.Throws<InvalidOperationException>(() => lifetime.GetRequiredService<TestService>());
    }

    [Fact]
    public void GetCancellationToken_UnregisteredService_ThrowsInvalidOperationException()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        Assert.Throws<InvalidOperationException>(() => lifetime.GetCancellationToken<TestService>());
    }

    [Fact]
    public void Methods_WhenDisposed_ThrowsObjectDisposedException()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        provider.Dispose();

        // assert
        Assert.Throws<ObjectDisposedException>(() => lifetime.GetService<TestService>());
        Assert.Throws<ObjectDisposedException>(() => lifetime.GetRequiredService<TestService>());
        Assert.Throws<ObjectDisposedException>(() => lifetime.GetCancellationToken<TestService>());
    }
}
