using Microsoft.Extensions.DependencyInjection;

namespace LifeTimes.Types;

public class ILifeTimeTest
{
    private class TestService
    {
    }
    private class TestServiceTrueCondition : IConditional
    {
        public ValueTask<bool> ConditionAsync(CancellationToken cancellationToken) => ValueTask.FromResult<bool>(true);
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
    private interface ITestServiceInterface
    {
    }
    private class TestServiceInterface : ITestServiceInterface
    {
    }
    private interface ITestServiceTrueConditionInterface : IConditional
    {
    }
    private class TestServiceTrueConditionInterface : ITestServiceTrueConditionInterface
    {
        public ValueTask<bool> ConditionAsync(CancellationToken cancellationToken) => ValueTask.FromResult<bool>(true);
    }

    [Fact]
    public async Task GetService_ReturnsNull()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) => { });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance = await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken);

        // assert
        Assert.Null(instance);
    }

    [Fact]
    public async Task GetRequiredService_ThrowsInvalidOperationException()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) => { });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken));
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
    public async Task Methods_WhenDisposed_ThrowsObjectDisposedException()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
        {
        });
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        await provider.DisposeAsync();

        // assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await lifetime.GetRequiredServiceAsync<TestService>(TestContext.Current.CancellationToken));
        Assert.Throws<ObjectDisposedException>(() => lifetime.GetCancellationToken<TestService>());
    }

    [Fact]
    public async Task GetService_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<TestService>(p, TimeSpan.FromMilliseconds(500));
                o.AddConditional<TestServiceTrueCondition>(p);
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken);
        var instance2 = await lifetime.GetServiceAsync<TestServiceTrueCondition>(TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }

    [Fact]
    public async Task GetService_WithInterface_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<ITestServiceInterface, TestServiceInterface>(p, TimeSpan.FromMilliseconds(500));
                o.AddConditional<ITestServiceTrueConditionInterface, TestServiceTrueConditionInterface>(p);
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = await lifetime.GetServiceAsync<ITestServiceInterface>(TestContext.Current.CancellationToken);
        var instance2 = await lifetime.GetServiceAsync<ITestServiceTrueConditionInterface>(TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }

    [Fact]
    public async Task Get_WithJustInterface_ThrowsInvalidOperationException()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<ITestServiceInterface>(p, TimeSpan.FromMilliseconds(500));
                o.AddConditional<ITestServiceTrueConditionInterface>(p);
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();

        // assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lifetime.GetRequiredServiceAsync<ITestServiceInterface>(TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lifetime.GetRequiredServiceAsync<ITestServiceTrueConditionInterface>(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetService_WithFactory_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<TestService>(() => new TestService(), TimeSpan.FromMilliseconds(500));
                o.AddConditional<TestServiceTrueCondition>(() => new TestServiceTrueCondition());
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = await lifetime.GetServiceAsync<TestService>(TestContext.Current.CancellationToken);
        var instance2 = await lifetime.GetServiceAsync<TestServiceTrueCondition>(TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }

    [Fact]
    public async Task GetService_WithFactoryAndInterface_ReturnsIntance()
    {
        // arrrange
        var services = new ServiceCollection();
        services.AddLifeTime((p, o) =>
            {
                o.AddTimed<ITestServiceInterface, TestServiceInterface>(() => new TestServiceInterface(), TimeSpan.FromMilliseconds(500));
                o.AddConditional<ITestServiceTrueConditionInterface, TestServiceTrueConditionInterface>(() => new TestServiceTrueConditionInterface());
            }
        );
        var provider = services.BuildServiceProvider();

        // act
        var lifetime = provider.GetRequiredService<ILifeTime>();
        var instance1 = await lifetime.GetServiceAsync<ITestServiceInterface>(TestContext.Current.CancellationToken);
        var instance2 = await lifetime.GetServiceAsync<ITestServiceTrueConditionInterface>(TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
    }
}
