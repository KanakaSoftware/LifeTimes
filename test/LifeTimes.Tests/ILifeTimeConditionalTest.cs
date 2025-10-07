using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

public class ILifeTimeConditionalTest
{
    private class TestServiceTrueCondition : IConditional
    {
        public bool Condition() => true;
    }

    private class TestServiceFlaseCondition : IConditional
    {
        public bool Condition() => false;
    }

    private class TestServiceDisposableTrueCondition : IDisposable, IConditional
    {
        public bool _disposed = false;

        public bool Condition() => true;

        public void Dispose() => _disposed = true;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Get_ReturnsSameInstance_WhenConditionFalse(bool useGetService)
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
            ? lifetime.GetService<TestServiceFlaseCondition>()
            : lifetime.GetRequiredService<TestServiceFlaseCondition>();
        var instance2 = useGetService
            ? lifetime.GetService<TestServiceFlaseCondition>()
            : lifetime.GetRequiredService<TestServiceFlaseCondition>();

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.Same(instance1, instance2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Get_ReturnsNewInstance_WhenConditionTrue(bool useGetService)
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
            ? lifetime.GetService<TestServiceTrueCondition>()
            : lifetime.GetRequiredService<TestServiceTrueCondition>();
        var instance2 = useGetService
            ? lifetime.GetService<TestServiceTrueCondition>()
            : lifetime.GetRequiredService<TestServiceTrueCondition>();

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
            o.AddConditional<TestServiceFlaseCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = useGetService
            ? lifetime.GetService<TestServiceFlaseCondition>()
            : lifetime.GetRequiredService<TestServiceFlaseCondition>();
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
    public void GetCancellationToken_IsTokenCancelled(bool useGetService)
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
                ? lifetime.GetService<TestServiceDisposableTrueCondition>()
                : lifetime.GetRequiredService<TestServiceDisposableTrueCondition>();
        var token = lifetime.GetCancellationToken<TestServiceDisposableTrueCondition>();
        var instance2 = useGetService
                ? lifetime.GetService<TestServiceDisposableTrueCondition>()
                : lifetime.GetRequiredService<TestServiceDisposableTrueCondition>();

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
            o.AddConditional<TestServiceFlaseCondition>(p);
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        TestServiceFlaseCondition? instance1 = null, instance2 = null;
        Parallel.Invoke(
            () =>
            {
                instance1 = useGetService
                    ? lifetime.GetService<TestServiceFlaseCondition>()
                    : lifetime.GetRequiredService<TestServiceFlaseCondition>();
            },
            () =>
            {
                instance2 = useGetService
                    ? lifetime.GetService<TestServiceFlaseCondition>()
                    : lifetime.GetRequiredService<TestServiceFlaseCondition>();
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
    public void Get_IsServiceDisposed(bool useGetService)
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
                ? lifetime.GetService<TestServiceDisposableTrueCondition>()
                : lifetime.GetRequiredService<TestServiceDisposableTrueCondition>();
        var instance2 = useGetService
                ? lifetime.GetService<TestServiceDisposableTrueCondition>()
                : lifetime.GetRequiredService<TestServiceDisposableTrueCondition>();

        // assert
        Assert.NotNull(instance1);
        Assert.True(instance1._disposed);
    }
}
