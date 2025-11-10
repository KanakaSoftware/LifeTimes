using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

public class ILifeTimeTimedTest
{
    private class TestService
    {
    }

    private class TestServiceDisposable : IAsyncDisposable
    {
        public bool _disposed = false;

        public ValueTask DisposeAsync()
        {
            _disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ReturnsSameInstanceWithinInterval(bool useGetService)
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
            ? await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken);
        var instance2 = useGetService
            ? await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken);

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
            ? await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromMilliseconds(300), TestContext.Current.CancellationToken);
        var instance2 = useGetService
            ? await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.NotSame(instance1, instance2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetCancellationToken_ReturnsCancellationToken(bool useGetService)
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
            ? await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken);
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
                ? await lifetime.GetServiceAsync<TestServiceDisposable>(TestContext.Current.CancellationToken)
                : await lifetime.GetRequiredServiceAsync<TestServiceDisposable>(TestContext.Current.CancellationToken);
        var token = lifetime.GetCancellationToken<TestServiceDisposable>();
        await Task.Delay(TimeSpan.FromMilliseconds(300), TestContext.Current.CancellationToken);

        // assert
        Assert.True(token.IsCancellationRequested);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_IsThreadSafe(bool useGetService)
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
        var tasks = new[]
        {
            Task.Run(
                async () =>
                    instance1 = useGetService
                        ? await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken)
                        : await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken)),
            Task.Run(
                async () =>
                    instance2 = useGetService
                        ? await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken)
                        : await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken))
        };
        await Task.WhenAll(tasks);

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
                ? await lifetime.GetServiceAsync<TestServiceDisposable>(TestContext.Current.CancellationToken)
                : await lifetime.GetRequiredServiceAsync<TestServiceDisposable>(TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromMilliseconds(300), TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance);
        Assert.True(instance._disposed);
    }
}
