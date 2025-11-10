using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

public class ILifeTimeConditionalTest
{
    private class TestServiceTrueCondition : IConditional
    {
        public ValueTask<bool> ConditionAsync(CancellationToken cancellationToken) => ValueTask.FromResult<bool>(true);
    }

    private class TestServiceFlaseCondition : IConditional
    {
        public ValueTask<bool> ConditionAsync(CancellationToken cancellationToken) => ValueTask.FromResult<bool>(false);
    }

    private class TestServiceDisposableTrueCondition : IAsyncDisposable, IConditional
    {
        public bool _disposed = false;

        public ValueTask<bool> ConditionAsync(CancellationToken cancellationToken) => ValueTask.FromResult<bool>(true);

        public ValueTask DisposeAsync()
        {
            _disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ReturnsSameInstance_WhenConditionFalse(bool useGetService)
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddConditional<TestServiceFlaseCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = useGetService
            ? await lifetime.GetServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken);
        var instance2 = useGetService
            ? await lifetime.GetServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.Same(instance1, instance2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ReturnsNewInstance_WhenConditionTrue(bool useGetService)
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
            o.AddConditional<TestServiceTrueCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = useGetService
            ? await lifetime.GetServiceAsync<TestServiceTrueCondition>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestServiceTrueCondition>(TestContext.Current.CancellationToken);
        var instance2 = useGetService
            ? await lifetime.GetServiceAsync<TestServiceTrueCondition>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestServiceTrueCondition>(TestContext.Current.CancellationToken);

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
            o.AddConditional<TestServiceFlaseCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = useGetService
            ? await lifetime.GetServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken)
            : await lifetime.GetRequiredServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken);
        var token = lifetime.GetCancellationToken<TestServiceFlaseCondition>();

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
            o.AddConditional<TestServiceFlaseCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        Assert.Throws<InvalidOperationException>(() => lifetime.GetCancellationToken<TestServiceFlaseCondition>());
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
            o.AddConditional<TestServiceDisposableTrueCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = useGetService
                ? await lifetime.GetServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken)
                : await lifetime.GetRequiredServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken);
        var token = lifetime.GetCancellationToken<TestServiceDisposableTrueCondition>();
        var instance2 = useGetService
                ? await lifetime.GetServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken)
                : await lifetime.GetRequiredServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken);

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
            o.AddConditional<TestServiceFlaseCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        TestServiceFlaseCondition? instance1 = null, instance2 = null;
        var tasks = new[]
        {
            Task.Run(
                async () =>
                    instance1 = useGetService
                        ? await lifetime.GetServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken)
                        : await lifetime.GetRequiredServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken)),
            Task.Run(
                async () =>
                    instance2 = useGetService
                        ? await lifetime.GetServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken)
                        : await lifetime.GetRequiredServiceAsync<TestServiceFlaseCondition>(TestContext.Current.CancellationToken))
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
            o.AddConditional<TestServiceDisposableTrueCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = useGetService
                ? await lifetime.GetServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken)
                : await lifetime.GetRequiredServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken);
        var instance2 = useGetService
                ? await lifetime.GetServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken)
                : await lifetime.GetRequiredServiceAsync<TestServiceDisposableTrueCondition>(TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance1);
        Assert.True(instance1._disposed);
    }
}
