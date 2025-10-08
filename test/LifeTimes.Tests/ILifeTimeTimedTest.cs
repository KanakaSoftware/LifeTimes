using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

public class ILifeTimeTimedTest
{
    private class TestService
    {
    }

    private class TestServiceDisposable : IDisposable
    {
        public bool _disposed = false;

        public void Dispose() => _disposed = true;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Get_ReturnsSameInstanceWithinInterval(bool useGetService)
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddTimed<TestService>(p, TimeSpan.FromMilliseconds(500));
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = useGetService
            ? lifetime.GetService<TestService>()
            : lifetime.GetRequiredService<TestService>();
        var instance2 = useGetService
            ? lifetime.GetService<TestService>()
            : lifetime.GetRequiredService<TestService>();

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.Same(instance1, instance2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ReturnsNewInstanceAfterInterval(bool useGetService)
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddTimed<TestService>(p, TimeSpan.FromMilliseconds(100));
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = useGetService
            ? lifetime.GetService<TestService>()
            : lifetime.GetRequiredService<TestService>();
        await Task.Delay(TimeSpan.FromMilliseconds(150), TestContext.Current.CancellationToken);
        var instance2 = useGetService
            ? lifetime.GetService<TestService>()
            : lifetime.GetRequiredService<TestService>();

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.NotSame(instance1, instance2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetCancellationToken_ReturnsCancellationToken(bool useGetService)
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddTimed<TestService>(p, TimeSpan.FromMilliseconds(100));
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = useGetService
            ? lifetime.GetService<TestService>()
            : lifetime.GetRequiredService<TestService>();
        var token = lifetime.GetCancellationToken<TestService>();

        // assert
        Assert.False(token.IsCancellationRequested);
    }

    [Fact]
    public void GetCancellationToken_BeforeGetService_ThrowsInvalidOperationException()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddTimed<TestService>(p, TimeSpan.FromMilliseconds(100));
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        Assert.Throws<InvalidOperationException>(() => lifetime.GetCancellationToken<TestService>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetCancellationToken_IsTokenCancelled(bool useGetService)
    {

        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddTimed<TestServiceDisposable>(p, TimeSpan.FromMilliseconds(100));
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = useGetService
                ? lifetime.GetService<TestServiceDisposable>()
                : lifetime.GetRequiredService<TestServiceDisposable>();
        var token = lifetime.GetCancellationToken<TestServiceDisposable>();
        await Task.Delay(TimeSpan.FromMilliseconds(150), TestContext.Current.CancellationToken);

        // assert
        Assert.True(token.IsCancellationRequested);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Get_IsThreadSafe(bool useGetService)
    {

        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddTimed<TestService>(p, TimeSpan.FromMilliseconds(500));
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        TestService? instance1 = null, instance2 = null;
        Parallel.Invoke(
            () =>
            {
                instance1 = useGetService
                    ? lifetime.GetService<TestService>()
                    : lifetime.GetRequiredService<TestService>();
            },
            () =>
            {
                instance2 = useGetService
                    ? lifetime.GetService<TestService>()
                    : lifetime.GetRequiredService<TestService>();
            }
        );

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.Same(instance1, instance2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_IsServiceDisposed(bool useGetService)
    {

        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddTimed<TestServiceDisposable>(p, TimeSpan.FromMilliseconds(100));
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = useGetService
                ? lifetime.GetService<TestServiceDisposable>()
                : lifetime.GetRequiredService<TestServiceDisposable>();
        await Task.Delay(TimeSpan.FromMilliseconds(150), TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance);
        Assert.True(instance._disposed);
    }
}
